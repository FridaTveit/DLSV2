
CREATE FUNCTION [dbo].[GetSelfAssessmentSummaryForCandidate]
(
	@CandidateID int,
	@SelfAssessmentID int
)
RETURNS @ResTable TABLE 
(
	CompetencyGroupID int,
	Confidence float,
	Relevance float
)

AS	  
BEGIN
INSERT INTO @ResTable
	SELECT CompetencyGroupID, [1] AS Confidence, [2] AS Relevance
FROM   (SELECT comp.CompetencyGroupID, sar.AssessmentQuestionID, sar.Result*1.0 AS Result
             FROM    Competencies AS comp INNER JOIN
                           SelfAssessmentResults AS sar ON comp.ID = sar.CompetencyID
             WHERE  (sar.SelfAssessmentID = @SelfAssessmentID) AND (sar.CandidateID = @CandidateID)) sr PIVOT (AVG(Result) FOR AssessmentQuestionID IN ([1], [2])) AS pvt
RETURN
END


GO
/****** Object:  UserDefinedFunction [dbo].[GetFilteredAPISeniorityID]    Script Date: 22/09/2020 09:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Kevin Whittaker
-- Create date: 21/09/2020
-- Description:	Returns calculated SeniorityID for Filtered API
-- =============================================
CREATE FUNCTION [dbo].[GetFilteredAPISeniorityID] 
(
	-- Add the parameters for the function here
	@SelfAssessmentID int,
	@CandidateID int
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Res int

	-- Add the T-SQL statements to compute the return value here
	SELECT @Res = (SELECT TOP (1) fsm.SeniorityID
FROM   (SELECT CompetencyGroupID, Confidence, [Relevance]
             FROM    dbo.GetSelfAssessmentSummaryForCandidate(@CandidateID, @SelfAssessmentID) AS GetSelfAssessmentSummaryForCandidate_1
             WHERE  (Confidence IS NOT NULL) AND ([Relevance] IS NOT NULL)) AS t INNER JOIN
             FilteredSeniorityMapping AS fsm ON t.CompetencyGroupID = fsm.CompetencyGroupID
ORDER BY t.[Relevance] - t.Confidence DESC)

	-- Return the result of the function
	RETURN @Res

END
GO
/****** Object:  StoredProcedure [dbo].[GetFilteredCompetencyResponsesForCandidate]    Script Date: 22/09/2020 09:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Kevin Whittaker
-- Create date: 22/09/2020
-- Description:	Returns user self assessment responses (AVG) for Filtered competency
-- =============================================
CREATE PROCEDURE [dbo].[GetFilteredCompetencyResponsesForCandidate]
	-- Add the parameters for the stored procedure here
	@SelfAssessmentID int,
	@CandidateID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT FilteredCompetencyID AS id, Case WHEN [2] = 0.0 THEN 0.1 WHEN [2] = 1.0 THEN 0.9 ELSE [2] END AS importance, Case WHEN [1] = 0.0 THEN 0.1 WHEN [1] = 1.0 THEN 0.9 ELSE [1] END AS confidence
FROM   (SELECT fcm.FilteredCompetencyID, sar.AssessmentQuestionID, sar.Result * 0.1 AS Result
FROM   FilteredComptenencyMapping AS fcm INNER JOIN
             Competencies AS com ON fcm.CompetencyID = com.ID INNER JOIN
             SelfAssessmentResults AS sar ON com.ID = sar.CompetencyID
             WHERE  (sar.SelfAssessmentID = @SelfAssessmentID) AND (sar.CandidateID = @CandidateID)) sr PIVOT (AVG(Result) FOR AssessmentQuestionID IN ([1], [2])) AS pvt
END
GO
/****** Object:  StoredProcedure [dbo].[GetFilteredProfileForCandidate]    Script Date: 22/09/2020 09:22:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Kevin Whittaker
-- Create date: 22/09/2020
-- Description:	Gets Filtered API profile for candidate
-- =============================================
CREATE PROCEDURE [dbo].[GetFilteredProfileForCandidate]
	-- Add the parameters for the stored procedure here
	@SelfAssessmentID int,
	@CandidateID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT [dbo].[GetFilteredAPISeniorityID] (
   @SelfAssessmentID,
  @CandidateID) AS seniority,
  (SELECT TOP (1) fms.SectorID 

FROM   FilteredSectorsMapping AS fms INNER JOIN 

             Candidates AS ca ON fms.JobGroupID = ca.JobGroupID 

WHERE (ca.CandidateID = @CandidateID)) AS sector,
(SELECT CASE WHEN AVG(Result) < 4 THEN 414 WHEN AVG(Result) < 7 THEN 413 WHEN AVG(Result) < 9 THEN 412 ELSE 411 END AS FunctionID 
FROM   SelfAssessmentResults 
WHERE (CandidateID = @CandidateID) AND (SelfAssessmentID = @SelfAssessmentID) AND (AssessmentQuestionID = 1) ) AS [function]
  
END
GO


