﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalLearningSolutions.Data.Models.External.Filtered;
using DigitalLearningSolutions.Web.ViewModels.LearningPortal.SelfAssessments.FilteredMgp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DigitalLearningSolutions.Web.Controllers.LearningPortalController
{
    public partial class LearningPortalController
    {
        private async Task<string> GetFilteredToken()
        {
            string candidateNum = GetCandidateNumber();
            string? filteredToken = null;
            if (Request.Cookies.ContainsKey("filtered-" + candidateNum))
            {
                filteredToken = Request.Cookies["filtered-" + candidateNum];
            }
            //filteredToken = GetCookie(candidateNum);
            if (filteredToken == null)
            {
                var accessToken = await filteredApiHelperService.GetUserAccessToken<AccessToken>(candidateNum);
                filteredToken = accessToken.Jwt_access_token.ToString();
                //SetCookie(candidateNum, filteredToken, 15);
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddMinutes(15));
                Response.Cookies.Append("filtered-" + candidateNum, filteredToken, cookieOptions);
            }
            return filteredToken;
        }
        [Route("/LearningPortal/SelfAssessment/{selfAssessmentId:int}/Filtered/Results")]
        public async Task<IActionResult> SelfAssessmentFilteredResults(int selfAssessmentId)
        {
            var assessment = selfAssessmentService.GetSelfAssessmentForCandidateById(GetCandidateId(), selfAssessmentId);
            if (assessment == null)
            {
                logger.LogWarning($"Attempt to display self assessment Filtered API results for candidate {GetCandidateId()} with no self assessment");
                return StatusCode(403);
            }
            selfAssessmentService.UpdateLastAccessed(assessment.Id, GetCandidateId());
            var filteredToken = await GetFilteredToken();
            var profile = selfAssessmentService.GetFilteredProfileForCandidateById(selfAssessmentId, GetCandidateId());
            var goals = selfAssessmentService.GetFilteredGoalsForCandidateId(selfAssessmentId, GetCandidateId()).ToList();
            var response = await filteredApiHelperService.UpdateProfileAndGoals(filteredToken, profile, goals);
            var model = new SelfAssessmentFilteredResultsViewModel()
             {
                 SelfAssessment = assessment,
                 CompetencyPlayLists = await filteredApiHelperService.GetPlayListsPoll<IEnumerable<PlayList>>(filteredToken, "playlist.FetchCompetencyPlaylists"),
                 RecommendedPlayLists = await filteredApiHelperService.GetPlayListsPoll<IEnumerable<PlayList>>(filteredToken, "playlist.FetchNexRexPlaylists"),
                 FavouritePlayList = await filteredApiHelperService.GetPlayList<PlayList>(filteredToken, "playlist.FetchFavouritePlaylist", null)
             };
            return View("SelfAssessments/FilteredMgp/FilteredResults", model);
        }
        [Route("/LearningPortal/SelfAssessment/{selfAssessmentId:int}/Filtered/Dashboard")]
        public async Task<IActionResult> FilteredRecommendations(int selfAssessmentId)
        {
            var assessment = selfAssessmentService.GetSelfAssessmentForCandidateById(GetCandidateId(), selfAssessmentId);
            if (assessment == null)
            {
                logger.LogWarning($"Attempt to display self assessment Filtered API results for candidate {GetCandidateId()} with no self assessment");
                return StatusCode(403);
            }
            selfAssessmentService.UpdateLastAccessed(assessment.Id, GetCandidateId());
            var filteredToken = await GetFilteredToken();
            var model = new SelfAssessmentFilteredResultsViewModel()
            {
                SelfAssessment = assessment,
                CompetencyPlayLists = await filteredApiHelperService.GetPlayListsPoll<IEnumerable<PlayList>>(filteredToken, "playlist.FetchCompetencyPlaylists"),
                RecommendedPlayLists = await filteredApiHelperService.GetPlayListsPoll<IEnumerable<PlayList>>(filteredToken, "playlist.FetchNexRexPlaylists"),
                FavouritePlayList = await filteredApiHelperService.GetPlayList<PlayList>(filteredToken, "playlist.FetchFavouritePlaylist", null)
            };
            return View("SelfAssessments/FilteredMgp/FilteredResults", model);
        }
        [Route("/LearningPortal/SelfAssessment/{selfAssessmentId:int}/Filtered/PlayList/{playListId}")]
        public async Task<IActionResult> FilteredCompetencyPlaylist(int selfAssessmentId, string playListId)
        {
            var filteredToken = await GetFilteredToken();
            var model = await filteredApiHelperService.GetPlayList<PlayList>(filteredToken, "playlist.FetchCompetencyPlaylist", playListId);
            return View("SelfAssessments/FilteredMgp/PlayList", model);
        }
        [Route("/LearningPortal/SelfAssessment/{selfAssessmentId:int}/Filtered/LearningAsset/{assetId}")]
        public async Task<IActionResult> FilteredLearningAsset(int selfAssessmentId, int assetId)
        {
            var filteredToken = await GetFilteredToken();
            var model = await filteredApiHelperService.GetLearningAsset<LearningAsset>(filteredToken, "playlist.GetAssets", assetId);
            return View("SelfAssessments/FilteredMgp/Asset", model);
        }
    }
}