﻿namespace DigitalLearningSolutions.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Dapper;
    using DigitalLearningSolutions.Data.Models.SelfAssessments;
    using DigitalLearningSolutions.Data.Models.External.Filtered;
    using Microsoft.Extensions.Logging;

    public interface ISelfAssessmentService
    {
        IEnumerable<CurrentSelfAssessment> GetSelfAssessmentsForCandidate(int candidateId);
        CurrentSelfAssessment? GetSelfAssessmentForCandidateById(int candidateId, int selfAssessmentId);
        Competency? GetNthCompetency(int n, int selfAssessmentId, int candidateId); // 1 indexed
        void SetResultForCompetency(int competencyId, int selfAssessmentId, int candidateId, int assessmentQuestionId, int result);
        IEnumerable<Competency> GetMostRecentResults(int selfAssessmentId, int candidateId);
        void UpdateLastAccessed(int selfAssessmentId, int candidateId);
        void IncrementLaunchCount(int selfAssessmentId, int candidateId);
        void SetUpdatedFlag(int selfAssessmentId, int candidateId, bool status);
        void SetBookmark(int selfAssessmentId, int candidateId, string bookmark);
        void SetCompleteByDate(int selfAssessmentId, int candidateId, DateTime? completeByDate);
        Profile? GetFilteredProfileForCandidateById(int candidateId, int selfAssessmentId);
        IEnumerable<Goal> GetFilteredGoalsForCandidateId(int candidateId, int selfAssessmentId);
        void LogAssetLaunch(int candidateId, int selfAssessmentId, LearningAsset learningAsset);
    }

    public class SelfAssessmentService : ISelfAssessmentService
    {
        private readonly IDbConnection connection;
        private readonly ILogger<SelfAssessmentService> logger;

        private const string LatestAssessmentResults =
            @"LatestAssessmentResults AS
                         (SELECT CompetencyID,
                                 AssessmentQuestionID,
                                 Result
                          FROM SelfAssessmentResults s
                                   INNER JOIN (
                              SELECT MAX(ID) as ID
                              FROM SelfAssessmentResults
                              WHERE CandidateID = @candidateId
                                AND SelfAssessmentID = @selfAssessmentId
                              GROUP BY CompetencyID,
                                       AssessmentQuestionID
                          ) t
                                              ON s.ID = t.ID
                          WHERE CandidateID = @candidateId
                            AND SelfAssessmentID = @selfAssessmentId
                         )";
        private const string CompetencyFields = @"C.ID       AS Id,
                                                  C.Description AS Description,
                                                  CG.Name       AS CompetencyGroup,
                                                  AQ.ID         AS Id,
                                                  AQ.Question,
                                                  AQ.MaxValueDescription,
                                                  AQ.MinValueDescription,
                                                  LAR.Result";

        private const string CompetencyTables =
            @"Competencies AS C
                        INNER JOIN CompetencyGroups AS CG
                            ON C.CompetencyGroupID = CG.ID
                        INNER JOIN CompetencyAssessmentQuestions AS CAQ
                            ON CAQ.CompetencyID = C.ID
                        INNER JOIN AssessmentQuestions AS AQ
                            ON AQ.ID = CAQ.AssessmentQuestionID
                        INNER JOIN CandidateAssessments AS CA
                            ON CA.SelfAssessmentID = @selfAssessmentId
                                   AND CA.CandidateID = @candidateId
                        LEFT OUTER JOIN LatestAssessmentResults AS LAR
                            ON LAR.CompetencyID = C.ID
                                   AND LAR.AssessmentQuestionID = AQ.ID
                        INNER JOIN SelfAssessmentStructure AS SAS
                            ON C.ID = SAS.CompetencyID
                                    AND SAS.SelfAssessmentID = @selfAssessmentId";

        public SelfAssessmentService(IDbConnection connection, ILogger<SelfAssessmentService> logger)
        {
            this.connection = connection;
            this.logger = logger;
        }
        public IEnumerable<CurrentSelfAssessment> GetSelfAssessmentsForCandidate(int candidateId)
        {
            return connection.Query<CurrentSelfAssessment>(
                @"SELECT CA.SelfAssessmentID AS Id,
                             SA.Name,
                             SA.Description,
SA.UseFilteredApi,
                             COUNT(C.ID)         AS NumberOfCompetencies,
                             CA.StartedDate,
                             CA.LastAccessed,
                             CA.CompleteByDate,
                             CA.UserBookmark,
                             CA.UnprocessedUpdates,
CA.LaunchCount, 1 AS IsSelfAssessment
                      FROM CandidateAssessments CA
                               JOIN SelfAssessments SA
                                    ON CA.SelfAssessmentID = SA.ID
                               INNER JOIN SelfAssessmentStructure AS SAS
                                          ON CA.SelfAssessmentID = SAS.SelfAssessmentID
                               INNER JOIN Competencies AS C
                                          ON SAS.CompetencyID = C.ID
                      WHERE CA.CandidateID = @candidateId AND CA.RemovedDate IS NULL AND CA.CompletedDate IS NULL
                      GROUP BY CA.SelfAssessmentID, SA.Name, SA.Description, SA.UseFilteredApi, CA.StartedDate, CA.LastAccessed, CA.CompleteByDate, CA.UserBookmark, CA.UnprocessedUpdates, CA.LaunchCount",
                new { candidateId }
            );
        }
        public CurrentSelfAssessment? GetSelfAssessmentForCandidateById(int candidateId, int selfAssessmentId)
        {
            return connection.QueryFirstOrDefault<CurrentSelfAssessment>(
                @"SELECT CA.SelfAssessmentID AS Id,
                             SA.Name,
                             SA.Description,
SA.UseFilteredApi,
                             COUNT(C.ID)         AS NumberOfCompetencies,
                             CA.StartedDate,
                             CA.LastAccessed,
                             CA.CompleteByDate,
                             CA.UserBookmark,
                             CA.UnprocessedUpdates,
CA.LaunchCount
                      FROM CandidateAssessments CA
                               JOIN SelfAssessments SA
                                    ON CA.SelfAssessmentID = SA.ID
                               INNER JOIN SelfAssessmentStructure AS SAS
                                          ON CA.SelfAssessmentID = SAS.SelfAssessmentID
                               INNER JOIN Competencies AS C
                                          ON SAS.CompetencyID = C.ID
                      WHERE CA.CandidateID = @candidateId AND CA.SelfAssessmentID = @selfAssessmentId AND CA.RemovedDate IS NULL AND CA.CompletedDate IS NULL
                      GROUP BY CA.SelfAssessmentID, SA.Name, SA.Description, SA.UseFilteredApi, CA.StartedDate, CA.LastAccessed, CA.CompleteByDate, CA.UserBookmark, CA.UnprocessedUpdates, CA.LaunchCount",
                new { candidateId, selfAssessmentId }
            );
        }

        public Competency? GetNthCompetency(int n, int selfAssessmentId, int candidateId)
        {
            Competency? competencyResult = null;
            return connection.Query<Competency, AssessmentQuestion, Competency>(
                $@"WITH CompetencyRowNumber AS
                     (SELECT ROW_NUMBER() OVER (ORDER BY CompetencyID) as RowNo,
                             CompetencyID
                      FROM SelfAssessmentStructure
                      WHERE SelfAssessmentID = @selfAssessmentId
                     ),
                     {LatestAssessmentResults}
                    SELECT {CompetencyFields}
                    FROM {CompetencyTables}
                        INNER JOIN CompetencyRowNumber AS CRN
                            ON CRN.CompetencyID = C.ID
                    WHERE CRN.RowNo = @n",
                (competency, assessmentQuestion) =>
                {
                    if (competencyResult == null)
                    {
                        competencyResult = competency;
                    }

                    competencyResult.AssessmentQuestions.Add(assessmentQuestion);
                    return competencyResult;
                },
                param: new { n, selfAssessmentId, candidateId }
            ).FirstOrDefault();
        }

        public void SetResultForCompetency(int competencyId, int selfAssessmentId, int candidateId, int assessmentQuestionId, int result)
        {
            if (result < 0 || result > 10)
            {
                logger.LogWarning(
                    "Not saving self assessment result as result is invalid. " +
                    $"{PrintResult(competencyId, selfAssessmentId, candidateId, assessmentQuestionId, result)}"
                );
                return;
            }

            var numberOfAffectedRows = connection.Execute(
                @"IF EXISTS (
                        SELECT * FROM CandidateAssessments AS CA
	                    INNER JOIN SelfAssessmentStructure AS SAS ON CA.SelfAssessmentID = SAS.SelfAssessmentID
	                    INNER JOIN Competencies AS C ON SAS.CompetencyID = C.ID
	                    INNER JOIN CompetencyAssessmentQuestions as CAQ ON SAS.CompetencyID = CAQ.CompetencyID
	                    WHERE CandidateID = @candidateId
                            AND CA.SelfAssessmentID = @selfAssessmentId
                            AND C.ID = @competencyId
                            AND CAQ.AssessmentQuestionID = @assessmentQuestionId
                    )
                    BEGIN
                        INSERT INTO SelfAssessmentResults VALUES(@candidateId, @selfAssessmentId, @competencyId, @assessmentQuestionId, @result, GETDATE())
                    END",
                new { competencyId, selfAssessmentId, candidateId, assessmentQuestionId, result }
            );

            if (numberOfAffectedRows < 1)
            {
                logger.LogWarning(
                    "Not saving self assessment result as db insert failed. " +
                    $"{PrintResult(competencyId, selfAssessmentId, candidateId, assessmentQuestionId, result)}"
                );
            }
        }

        public IEnumerable<Competency> GetMostRecentResults(int selfAssessmentId, int candidateId)
        {
            var result = connection.Query<Competency, AssessmentQuestion, Competency>(
                $@"WITH {LatestAssessmentResults}
                    SELECT {CompetencyFields}
                    FROM {CompetencyTables}",
                (competency, assessmentQuestion) =>
                {
                    competency.AssessmentQuestions.Add(assessmentQuestion);
                    return competency;
                },
                param: new { selfAssessmentId, candidateId }
            );
            return result.GroupBy(competency => competency.Id).Select(group =>
            {
                var groupedCompetency = group.First();
                groupedCompetency.AssessmentQuestions = group.Select(competency => competency.AssessmentQuestions.Single()).ToList();
                return groupedCompetency;
            });
        }

        public void UpdateLastAccessed(int selfAssessmentId, int candidateId)
        {
            var numberOfAffectedRows = connection.Execute(
                @"UPDATE CandidateAssessments SET LastAccessed = GETDATE()
                      WHERE SelfAssessmentID = @selfAssessmentId AND CandidateID = @candidateId",
                new { selfAssessmentId, candidateId }
            );

            if (numberOfAffectedRows < 1)
            {
                logger.LogWarning(
                    "Not updating self assessment last accessed date as db update failed. " +
                    $"Self assessment id: {selfAssessmentId}, candidate id: {candidateId}"
                );
            }
        }

        public void SetCompleteByDate(int selfAssessmentId, int candidateId, DateTime? completeByDate)
        {
            var numberOfAffectedRows = connection.Execute(
                @"UPDATE CandidateAssessments
                        SET CompleteByDate = @date
                        WHERE SelfAssessmentID = @selfAssessmentId
                          AND CandidateID = @candidateId",
                new { date = completeByDate, selfAssessmentId, candidateId }
            );

            if (numberOfAffectedRows < 1)
            {
                logger.LogWarning(
                    "Not setting self assessment complete by date as db update failed. " +
                    $"Self assessment id: {selfAssessmentId}, candidate id: {candidateId}, complete by date: {completeByDate}"
                );
            }
        }

        private static string PrintResult(int competencyId, int selfAssessmentId, int candidateId, int assessmentQuestionId, int result)
        {
            return $"Competency id: {competencyId}, self assessment id: {selfAssessmentId}, candidate id: {candidateId}, " +
                   $"assessment question id: {assessmentQuestionId}, result: {result}";
        }
        public Profile GetFilteredProfileForCandidateById(int selfAssessmentId, int candidateId)
        {
            return connection.QueryFirstOrDefault<Profile>("GetFilteredProfileForCandidate", new { selfAssessmentId, candidateId }, commandType: CommandType.StoredProcedure);
        }
        public IEnumerable<Goal> GetFilteredGoalsForCandidateId(int selfAssessmentId, int candidateId)
        {
            return connection.Query<Goal>("GetFilteredCompetencyResponsesForCandidate", new { selfAssessmentId, candidateId }, commandType: CommandType.StoredProcedure);
        }

        public void SetUpdatedFlag(int selfAssessmentId, int candidateId, bool status)
        {
            var numberOfAffectedRows = connection.Execute(
                @"UPDATE CandidateAssessments
                        SET UnprocessedUpdates = @status
                        WHERE SelfAssessmentID = @selfAssessmentId
                          AND CandidateID = @candidateId",
                new { status, selfAssessmentId, candidateId }
            );

            if (numberOfAffectedRows < 1)
            {
                logger.LogWarning(
                    "Not setting self assessment updated flag as db update failed. " +
                    $"Self assessment id: {selfAssessmentId}, candidate id: {candidateId}, status: {status}"
                );
            }
        }

        public void SetBookmark(int selfAssessmentId, int candidateId, string bookmark)
        {
            var numberOfAffectedRows = connection.Execute(
                @"UPDATE CandidateAssessments
                        SET UserBookmark = @bookmark
                        WHERE SelfAssessmentID = @selfAssessmentId
                          AND CandidateID = @candidateId",
                new { bookmark, selfAssessmentId, candidateId }
            );

            if (numberOfAffectedRows < 1)
            {
                logger.LogWarning(
                    "Not setting self assessment bookmark as db update failed. " +
                    $"Self assessment id: {selfAssessmentId}, candidate id: {candidateId}, bookmark: {bookmark}"
                );
            }
        }

        public void IncrementLaunchCount(int selfAssessmentId, int candidateId)
        {
            var numberOfAffectedRows = connection.Execute(
               @"UPDATE CandidateAssessments SET LaunchCount = LaunchCount+1
                      WHERE SelfAssessmentID = @selfAssessmentId AND CandidateID = @candidateId",
               new { selfAssessmentId, candidateId }
           );

            if (numberOfAffectedRows < 1)
            {
                logger.LogWarning(
                    "Not updating self assessment launch count as db update failed. " +
                    $"Self assessment id: {selfAssessmentId}, candidate id: {candidateId}"
                );
            }
        }

        public void LogAssetLaunch(int candidateId, int selfAssessmentId, LearningAsset learningAsset)
        {
            connection.Execute("UpdateFilteredLearningActivity",
                new
                {
                    FilteredAssetID = learningAsset.Id,
                    Title = learningAsset.Title,
                    Description = learningAsset.Description,
                    DirectUrl = learningAsset.DirectUrl,
                    Type = learningAsset.TypeLabel,
                    Provider = learningAsset.Provider.Name,
                    Duration = learningAsset.LengthSeconds,
                    ActualDuration = learningAsset.LengthSeconds,
                    CandidateId = candidateId,
                    SelfAssessmentID = selfAssessmentId,
                    Completed = learningAsset.Completed,
                    Outcome = learningAsset.CompletedStatus,
                    Bookmark = learningAsset.IsFavourite
                }, commandType: CommandType.StoredProcedure);
        }
    }
}
