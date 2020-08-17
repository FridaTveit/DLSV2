﻿namespace DigitalLearningSolutions.Web.Controllers.LearningPortalController
{
    using System.Collections.Generic;
    using DigitalLearningSolutions.Data.Models;
    using DigitalLearningSolutions.Web.ViewModels.LearningPortal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public partial class LearningPortalController
    {
        public IActionResult SelfAssessment()
        {
            var selfAssessment = selfAssessmentService.GetSelfAssessmentForCandidate(GetCandidateId());

            if (selfAssessment == null)
            {
                logger.LogWarning($"Attempt to display self assessment description for candidate {GetCandidateId()} with no self assessment");
                return StatusCode(403);
            }

            var model = new SelfAssessmentDescriptionViewModel(selfAssessment);
            return View("SelfAssessment/SelfAssessmentDescription", model);
        }

        [Route("/LearningPortal/SelfAssessment/{competencyNumber:int}")]
        public IActionResult SelfAssessmentCompetency(int competencyNumber)
        {
            var assessment = selfAssessmentService.GetSelfAssessmentForCandidate(GetCandidateId());
            if (assessment == null)
            {
                logger.LogWarning($"Attempt to display self assessment competency for candidate {GetCandidateId()} with no self assessment");
                return StatusCode(403);
            }

            var competency = selfAssessmentService.GetNthCompetency(competencyNumber, assessment.Id, GetCandidateId());
            if (competency == null)
            {
                return RedirectToAction("Current"); // TODO HEEDLS-65: Redirect to review page
            }

            var model = new SelfAssessmentCompetencyViewModel(assessment, competency, competencyNumber);
            return View("SelfAssessment/SelfAssessmentCompetency", model);
        }

        [HttpPost]
        [Route("/LearningPortal/SelfAssessment/{competencyNumber:int}")]
        public IActionResult SelfAssessmentCompetency(ICollection<AssessmentQuestion> assessmentQuestions, int competencyNumber, int competencyId)
        {
            var assessment = selfAssessmentService.GetSelfAssessmentForCandidate(GetCandidateId());
            if (assessment == null)
            {
                logger.LogWarning($"Attempt to set self assessment competency for candidate {GetCandidateId()} with no self assessment");
                return StatusCode(403);
            }

            foreach (var assessmentQuestion in assessmentQuestions)
            {
                selfAssessmentService.SetResultForCompetency(
                    competencyId,
                    assessment.Id,
                    GetCandidateId(),
                    assessmentQuestion.Id,
                    assessmentQuestion.Result.Value
                );
            }

            return RedirectToAction("SelfAssessmentCompetency", new { competencyNumber = competencyNumber + 1 });
        }
    }
}