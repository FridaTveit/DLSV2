﻿namespace DigitalLearningSolutions.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Dapper;
    using DigitalLearningSolutions.Data.Models;

    public interface ICourseService
    {
        IEnumerable<CurrentCourse> GetCurrentCourses(int candidateId);
        IEnumerable<CompletedCourse> GetCompletedCourses(int candidateId);
        IEnumerable<Course> GetAvailableCourses();
        void SetCompleteByDate(int progressId, int candidateId, DateTime? completeByDate);
        void RemoveCurrentCourse(int progressId, int candidateId);
    }

    public class CourseService : ICourseService
    {
        private readonly IDbConnection connection;

        public CourseService(IDbConnection connection)
        {
            this.connection = connection;
        }

        public IEnumerable<CurrentCourse> GetCurrentCourses(int candidateId)
        {
            return connection.Query<CurrentCourse>("GetCurrentCoursesForCandidate_V2", new { candidateId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<CompletedCourse> GetCompletedCourses(int candidateId)
        {
            return connection.Query<CompletedCourse>("GetCompletedCoursesForCandidate", new { candidateId }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<Course> GetAvailableCourses()
        {
            return connection.Query<Course>(@"
                SELECT ApplicationID AS Id, ApplicationName AS Name FROM Applications WHERE CreatedById = 2223
            ");
        }

        public void SetCompleteByDate(int progressId, int candidateId, DateTime? completeByDate)
        {
            connection.Execute(
                @"UPDATE Progress
                        SET CompleteByDate = @date
                        WHERE ProgressID = @progressId
                          AND CandidateID = @candidateId",
                new { date = completeByDate, progressId, candidateId }
                );
        }

        public void RemoveCurrentCourse(int progressId, int candidateId)
        {
            connection.Execute(
            @"UPDATE Progress
                    SET RemovedDate = getUTCDate(),
                        RemovalMethodID = 1
                    WHERE ProgressID = @progressId
                      AND CandidateID = @candidateId
                ",
            new { progressId, candidateId }
            );
        }
    }
}
