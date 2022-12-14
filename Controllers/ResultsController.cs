using Auth0.AspNetCore.Authentication;
using Lab1.DB;
using Lab1.Models;
using Lab1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IDataBase _dataBase;

        public ResultsController(IDataBase dataBase)
        {
            _dataBase = dataBase;
        }

        [AllowAnonymous]
        public IActionResult Rounds()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Role = "admin";
            }
            else if (User.IsInRole("User"))
            {
                ViewBag.Role = "user";
            }
            else
            {
                ViewBag.Role = "anonymous";
            }
            ViewBag.UserId = User.Identity.Name;
            Scedule scedule = _dataBase.GetSceduleData();
            ViewBag.Scedule = JsonConvert.SerializeObject(scedule);

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Rounds(string firstTeam, string secondTeam, int teamOneGoals, int TeamTwoGoals, int roundId, int matchId)
        {
            Match match = new Match()
            {
                firstTeam = firstTeam,
                secondTeam = secondTeam,
                goalsFirstTeam = teamOneGoals,
                goalsSecondTeam = TeamTwoGoals
            };

            _dataBase.UpdateMatchData(match, roundId, matchId);

            return Rounds();
        }

        [Authorize]
        [HttpPost]
        public IActionResult DeleteComment(int deleteCommentRoundId, int deleteCommentCommentId)
        {
            Scedule scedule = _dataBase.GetSceduleData();
            if (User.IsInRole("Admin"))
            {
                scedule.rounds[deleteCommentRoundId].comments.RemoveAt(deleteCommentCommentId);
                _dataBase.UpdateRoundCommentsData(scedule.rounds[deleteCommentRoundId].comments, deleteCommentRoundId);
            }
            else
            {
                if (scedule.rounds[deleteCommentRoundId].comments[deleteCommentCommentId].ownerId.Equals(User.Identity.Name))
                {
                    scedule.rounds[deleteCommentRoundId].comments.RemoveAt(deleteCommentCommentId);
                    _dataBase.UpdateRoundCommentsData(scedule.rounds[deleteCommentRoundId].comments, deleteCommentRoundId);
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.Role = "admin";
            }
            else if (User.IsInRole("User"))
            {
                ViewBag.Role = "user";
            }
            else
            {
                ViewBag.Role = "anonymous";
            }
            ViewBag.UserId = User.Identity.Name;
            scedule = _dataBase.GetSceduleData();
            ViewBag.Scedule = JsonConvert.SerializeObject(scedule);

            return View("Rounds");
        }

        [Authorize]
        [HttpPost]
        public IActionResult EditComment(string commentText, int editCommentRoundId, int editCommentCommentId)
        {
            Scedule scedule = _dataBase.GetSceduleData();
            if (User.IsInRole("User"))
            {

                if(editCommentCommentId >= 0)
                {
                    if (scedule.rounds[editCommentRoundId].comments[editCommentCommentId].ownerId == User.Identity.Name)
                    {
                        scedule.rounds[editCommentRoundId].comments[editCommentCommentId].commentText = commentText;
                        _dataBase.UpdateRoundCommentsData(scedule.rounds[editCommentRoundId].comments, editCommentRoundId);
                    }
                }
                else
                {
                    scedule.rounds[editCommentRoundId].comments.Add(new Comment()
                    {
                        ownerId = User.Identity.Name,
                        commentText = commentText,
                        timeCreated = DateTime.Now.ToString()
                    });
                    _dataBase.UpdateRoundCommentsData(scedule.rounds[editCommentRoundId].comments, editCommentRoundId);
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.Role = "admin";
            }
            else if (User.IsInRole("User"))
            {
                ViewBag.Role = "user";
            }
            else
            {
                ViewBag.Role = "anonymous";
            }
            ViewBag.UserId = User.Identity.Name;
            scedule = _dataBase.GetSceduleData();
            ViewBag.Scedule = JsonConvert.SerializeObject(scedule);

            return View("Rounds");
        }

        [AllowAnonymous]
        public IActionResult Standings()
        {
            Scedule scedule = _dataBase.GetSceduleData();
            StandingsViewModel standings = new StandingsViewModel();
            Dictionary<string, StandingsRow> standingDictionary = new Dictionary<string, StandingsRow>();

            foreach(Round round in scedule.rounds)
            {
                foreach(Match match in round.matches)
                {
                    if (match.goalsFirstTeam < 0 || match.goalsSecondTeam < 0) continue;

                    int firstTeamWon = match.goalsFirstTeam > match.goalsSecondTeam ? 1
                        : match.goalsFirstTeam == match.goalsSecondTeam ? 0 : -1;

                    int goalDifference = match.goalsFirstTeam - match.goalsSecondTeam;

                    if (!standingDictionary.ContainsKey(match.firstTeam))
                    {
                        standingDictionary.Add(match.firstTeam, new StandingsRow(match.firstTeam));
                    }

                    if (!standingDictionary.ContainsKey(match.secondTeam))
                    {
                        standingDictionary.Add(match.secondTeam, new StandingsRow(match.secondTeam));
                    }

                    standingDictionary[match.firstTeam].Games += 1;
                    standingDictionary[match.firstTeam].Wins += firstTeamWon == 1 ? 1 : 0;
                    standingDictionary[match.firstTeam].Loses += firstTeamWon == -1 ? 1 : 0;
                    standingDictionary[match.firstTeam].Draws += firstTeamWon == 0 ? 1 : 0;
                    standingDictionary[match.firstTeam].GoalDifference += goalDifference;
                    standingDictionary[match.firstTeam].Points += firstTeamWon == 0 ? 1 : firstTeamWon > 0 ? 3 : 0;

                    standingDictionary[match.secondTeam].Games += 1;
                    standingDictionary[match.secondTeam].Wins += firstTeamWon == -1 ? 1 : 0;
                    standingDictionary[match.secondTeam].Loses += firstTeamWon == 1 ? 1 : 0;
                    standingDictionary[match.secondTeam].Draws += firstTeamWon == 0 ? 1 : 0;
                    standingDictionary[match.secondTeam].GoalDifference += goalDifference * -1;
                    standingDictionary[match.secondTeam].Points += firstTeamWon == 0 ? 1 : firstTeamWon < 0 ? 3 : 0;
                }
            }

            foreach(KeyValuePair<string, StandingsRow> row in standingDictionary)
            {
                standings.Standings.Add(row.Value);
            }
            standings.Standings.Sort((s1, s2) =>
            {
                int same = s2.Points.CompareTo(s1.Points);
                if(same == 0)
                {
                    same = s2.GoalDifference.CompareTo(s1.GoalDifference);
                }
                return same;
            });
            for (int i = 0; i < standings.Standings.Count; i++) 
            {
                standings.Standings[i].OrderId = i + 1;
            }

            return View(standings);
        }
    }
}
