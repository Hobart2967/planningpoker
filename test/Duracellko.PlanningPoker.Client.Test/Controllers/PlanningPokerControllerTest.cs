﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    [TestClass]
    public class PlanningPokerControllerTest
    {
        private CultureInfo _originalCultureInfo;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalCultureInfo = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_originalCultureInfo != null)
            {
                CultureInfo.CurrentCulture = _originalCultureInfo;
                _originalCultureInfo = null;
            }
        }

        [TestMethod]
        public void Constructor_IsConnected_False()
        {
            var target = CreateController();

            Assert.IsFalse(target.IsConnected);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_TeamNameIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberName_IsNotScrumMaster()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberNameIsLowerCase_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, "test member");

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.MemberType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterNameIsUpperCase_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, "TEST SCRUM MASTER");

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ScrumMasterType, target.User.Type);
            Assert.IsTrue(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_ObserverName_UserIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ObserverName);

            Assert.AreEqual(scrumTeam, target.ScrumTeam);
            Assert.AreEqual(PlanningPokerData.TeamName, target.TeamName);
            Assert.AreEqual(PlanningPokerData.ObserverName, target.User.Name);
            Assert.AreEqual(PlanningPokerData.ObserverType, target.User.Type);
            Assert.IsFalse(target.IsScrumMaster);
        }

        [TestMethod]
        public async Task InitializeTeam_LastMessageId_IsMinus1()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(-1, target.LastMessageId);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_ScrumMasterIsSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(PlanningPokerData.ScrumMasterName, target.ScrumMaster.Name);
            Assert.IsFalse(target.ScrumMaster.Estimating);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(1, target.Members.Count());
            Assert.AreEqual(PlanningPokerData.MemberName, target.Members.First().Name);
            Assert.IsFalse(target.Members.First().Estimating);
            Assert.AreEqual(1, target.Observers.Count());
            Assert.AreEqual(PlanningPokerData.ObserverName, target.Observers.First().Name);
            Assert.IsFalse(target.Observers.First().Estimating);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeamWith4MembersAnd3Observers_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Name = "me", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "1st Member", Type = PlanningPokerData.MemberType });
            scrumTeam.Members.Add(new TeamMember { Name = "XYZ", Type = PlanningPokerData.MemberType });
            scrumTeam.Observers.Add(new TeamMember { Name = "ABC", Type = PlanningPokerData.ObserverType });
            scrumTeam.Observers.Add(new TeamMember { Name = "Hello, World!", Type = PlanningPokerData.ObserverType });

            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            var expectedMembers = new string[] { "1st Member", "me", PlanningPokerData.MemberName, "XYZ" };
            CollectionAssert.AreEqual(expectedMembers, target.Members.Select(m => m.Name).ToList());
            var expectedObservers = new string[] { "ABC", "Hello, World!", PlanningPokerData.ObserverName };
            CollectionAssert.AreEqual(expectedObservers, target.Observers.Select(m => m.Name).ToList());

            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeamWithMembersSetToNull_MembersAndObserversAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members = null;
            scrumTeam.Observers = null;
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            CollectionAssert.AreEqual(Array.Empty<string>(), target.Members.ToList());
            CollectionAssert.AreEqual(Array.Empty<string>(), target.Observers.ToList());
            Assert.IsNotNull(target.ScrumTeam.Members);
            Assert.AreEqual(0, target.ScrumTeam.Members.Count);
            Assert.IsNotNull(target.ScrumTeam.Observers);
            Assert.AreEqual(0, target.ScrumTeam.Observers.Count);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_AvailableEstimationsAreSet()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            var expectedEstimations = new double?[] { 0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100, double.PositiveInfinity, null };
            CollectionAssert.AreEqual(expectedEstimations, target.AvailableEstimations.ToList());
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndInitialState_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimationInProgress_CanCancelEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimationFinished_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumMasterAndEstimationCanceled_CanStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndInitialState_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimationInProgress_CannotCancelEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimationFinished_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_MemberAndEstimationCanceled_CannotStartEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationCanceled;
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAnd5Estimations_5Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 8 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" },
                    Estimation = new Estimation { Value = 8 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = 3 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = 8 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                    Estimation = new Estimation { Value = 2 }
                }
            };
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);

            var estimations = target.Estimations.ToList();
            Assert.AreEqual(5, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(8.0, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(8.0, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual("Tester", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(8.0, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(2.0, estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(3.0, estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndMemberWithoutEstimation_4Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 0 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" },
                    Estimation = new Estimation { Value = null }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                }
            };

            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = new Estimation { Value = double.PositiveInfinity }
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);

            var estimations = target.Estimations.ToList();
            Assert.AreEqual(5, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(0.0, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual("Tester", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndSameEstimationCount_6Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = 20 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                    Estimation = new Estimation { Value = 0 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" },
                    Estimation = new Estimation { Value = 13 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = 13 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 0 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" },
                    Estimation = new Estimation { Value = 20 }
                }
            };

            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = null
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);

            var estimations = target.Estimations.ToList();
            Assert.AreEqual(6, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(0.0, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(0.0, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(13.0, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual("Tester 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(13.0, estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(20.0, estimation.Estimation);

            estimation = estimations[5];
            Assert.AreEqual("Tester 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(20.0, estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndSameEstimationCountWithNull_6Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" });
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>
            {
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.ScrumMasterType, Name = PlanningPokerData.ScrumMasterName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" },
                    Estimation = new Estimation { Value = null }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 1" },
                    Estimation = new Estimation { Value = null }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = PlanningPokerData.MemberName },
                    Estimation = new Estimation { Value = double.PositiveInfinity }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" },
                    Estimation = new Estimation { Value = 5 }
                },
                new EstimationResultItem
                {
                    Member = new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester 2" },
                    Estimation = new Estimation { Value = 5 }
                }
            };
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);

            var estimations = target.Estimations.ToList();
            Assert.AreEqual(6, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(5.0, estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual("Tester 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(5.0, estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[3];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.AreEqual(double.PositiveInfinity, estimation.Estimation);

            estimation = estimations[4];
            Assert.AreEqual("Developer 2", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[5];
            Assert.AreEqual("Tester 1", estimation.MemberName);
            Assert.IsTrue(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationFinishedAndEmptyEstimationsList_NoEstimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationFinished;
            scrumTeam.EstimationResult = new List<EstimationResultItem>();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);

            Assert.AreEqual(0, target.Estimations.Count());
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAnd4Participants_3Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 1" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Developer 2" });
            scrumTeam.Members.Add(new TeamMember { Type = PlanningPokerData.MemberType, Name = "Tester" });
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.ScrumMasterName, Estimated = true },
                new EstimationParticipantStatus { MemberName = "Tester", Estimated = false },
                new EstimationParticipantStatus { MemberName = "Developer 1", Estimated = true },
                new EstimationParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = true }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = new Estimation { Value = 8 }
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoObserverIsEstimating(target);

            var estimations = target.Estimations.ToList();
            Assert.AreEqual(3, estimations.Count);

            var estimation = estimations[0];
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[1];
            Assert.AreEqual("Developer 1", estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            estimation = estimations[2];
            Assert.AreEqual(PlanningPokerData.MemberName, estimation.MemberName);
            Assert.IsFalse(estimation.HasEstimation);
            Assert.IsNull(estimation.Estimation);

            Assert.IsFalse(target.ScrumMaster.Estimating);
            AssertMemberIsEstimating(target, "Tester", true);
            AssertMemberIsEstimating(target, "Developer 1", false);
            AssertMemberIsEstimating(target, PlanningPokerData.MemberName, false);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAnd0Participants_0Estimations()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>();
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = null
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);

            Assert.AreEqual(0, target.Estimations.Count());
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndParticipantsListIsNull_EstimationsIsNull()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = null
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.ScrumMasterName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsTrue(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);

            Assert.IsNull(target.Estimations);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndMemberInParticipantsAndNotEstimated_CanSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = false }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = null
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsTrue(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoObserverIsEstimating(target);

            Assert.IsFalse(target.ScrumMaster.Estimating);
            AssertMemberIsEstimating(target, PlanningPokerData.MemberName, true);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndMemberInParticipantsAndEstimated_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.MemberName, Estimated = true }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = new Estimation { Value = 1 }
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoMemberIsEstimating(target);
        }

        [TestMethod]
        public async Task InitializeTeam_EstimationInProgressAndMemberNotInParticipants_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            scrumTeam.EstimationParticipants = new List<EstimationParticipantStatus>
            {
                new EstimationParticipantStatus { MemberName = PlanningPokerData.ScrumMasterName, Estimated = false }
            };
            var reconnectTeamResult = new ReconnectTeamResult
            {
                ScrumTeam = scrumTeam,
                LastMessageId = 22,
                SelectedEstimation = null
            };
            var target = CreateController();

            await target.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName);

            Assert.IsFalse(target.CanStartEstimation);
            Assert.IsFalse(target.CanCancelEstimation);
            Assert.IsFalse(target.CanSelectEstimation);
            Assert.IsTrue(target.IsConnected);
            AssertNoObserverIsEstimating(target);

            Assert.IsTrue(target.ScrumMaster.Estimating);
            AssertMemberIsEstimating(target, PlanningPokerData.MemberName, false);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_NotifyPropertyChanged()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();
            propertyChangedCounter.Target = target;

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.AreEqual(1, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task InitializeTeam_ScrumTeam_SetCredentialsAsync()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            MemberCredentials memberCredentials = null;
            memberCredentialsStore.Setup(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()))
                .Callback<MemberCredentials>(c => memberCredentials = c)
                .Returns(Task.CompletedTask);
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()));
            Assert.IsNotNull(memberCredentials);
            Assert.AreEqual(PlanningPokerData.TeamName, memberCredentials.TeamName);
            Assert.AreEqual(PlanningPokerData.ScrumMasterName, memberCredentials.MemberName);
        }

        [TestMethod]
        public async Task InitializeTeam_ReconnectTeamResult_SetCredentialsAsync()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            MemberCredentials memberCredentials = null;
            memberCredentialsStore.Setup(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()))
                .Callback<MemberCredentials>(c => memberCredentials = c)
                .Returns(Task.CompletedTask);
            var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
            var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(reconnectTeamResult, "test member");

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(It.IsAny<MemberCredentials>()));
            Assert.IsNotNull(memberCredentials);
            Assert.AreEqual(PlanningPokerData.TeamName, memberCredentials.TeamName);
            Assert.AreEqual(PlanningPokerData.MemberName, memberCredentials.MemberName);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_DisconnectTeam()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.Disconnect();

            planningPokerClient.Verify(o => o.DisconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task Disconnect_Initialized_IsConnectedIsFalse()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            Assert.IsFalse(target.IsConnected);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);

            Assert.IsTrue(target.IsConnected);

            await target.Disconnect();

            Assert.IsFalse(target.IsConnected);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_SetCredentialsToNull()
        {
            var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.Disconnect();

            memberCredentialsStore.Verify(o => o.SetCredentialsAsync(null));
        }

        [TestMethod]
        public async Task Disconnect_Initialized_NotifyPropertyChanged()
        {
            var propertyChangedCounter = new PropertyChangedCounter();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();
            propertyChangedCounter.Target = target;

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.Disconnect();

            Assert.AreEqual(2, propertyChangedCounter.Count);
        }

        [TestMethod]
        public async Task Disconnect_Initialized_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.DisconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var result = target.Disconnect();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task DisconnectMember_MemberName_DisconnectTeam()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.DisconnectMember(PlanningPokerData.MemberName);

            planningPokerClient.Verify(o => o.DisconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, It.IsAny<CancellationToken>()));
            Assert.IsTrue(target.IsConnected);
        }

        [TestMethod]
        public async Task DisconnectMember_ScrumMasterName_ArgumentException()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.DisconnectMember(PlanningPokerData.ScrumMasterName));
        }

        [TestMethod]
        public async Task DisconnectMember_Null_ArgumentNullException()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.DisconnectMember(null));
        }

        [TestMethod]
        public async Task DisconnectMember_Empty_ArgumentNullException()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.DisconnectMember(string.Empty));
        }

        [TestMethod]
        public async Task DisconnectMember_MemberName_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.DisconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var result = target.DisconnectMember(PlanningPokerData.MemberName);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task StartEstimation_CanStartEstimation_StartEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.StartEstimation();

            planningPokerClient.Verify(o => o.StartEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task StartEstimation_CannotStartEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.StartEstimation();

            planningPokerClient.Verify(o => o.StartEstimation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task StartEstimation_CanStartEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.StartEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var result = target.StartEstimation();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task CancelEstimation_CanCancelEstimation_CancelEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.CancelEstimation();

            planningPokerClient.Verify(o => o.CancelEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task CancelEstimation_CannotCancelEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            await target.CancelEstimation();

            planningPokerClient.Verify(o => o.CancelEstimation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task CancelEstimation_CanCancelEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.CancelEstimation(PlanningPokerData.TeamName, It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            scrumTeam.State = TeamState.EstimationInProgress;
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var result = target.CancelEstimation();

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        [TestMethod]
        public async Task SelectEstimation_5AndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(5);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, 5, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_PositiveInfinityAndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(double.PositiveInfinity);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.MemberName, double.PositiveInfinity, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_NullAndCanSelectEstimation_SelectEstimationOnService()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.MemberName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            await target.SelectEstimation(null);

            planningPokerClient.Verify(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.MemberName, null, It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SelectEstimation_CannotSelectEstimation_DoNothing()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            await target.SelectEstimation(5);

            planningPokerClient.Verify(o => o.SubmitEstimation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double?>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task SelectEstimation_Selects5_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimation);

            await target.SelectEstimation(5);

            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public async Task SelectEstimation_SelectsPositiveInfinity_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimation);

            await target.SelectEstimation(double.PositiveInfinity);

            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public async Task SelectEstimation_SelectsNull_CannotSelectEstimation()
        {
            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController();

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            Assert.IsTrue(target.CanSelectEstimation);

            await target.SelectEstimation(null);

            Assert.IsFalse(target.CanSelectEstimation);
        }

        [TestMethod]
        public async Task SelectEstimation_CanSelectEstimation_ShowsBusyIndicator()
        {
            var planningPokerClient = new Mock<IPlanningPokerClient>();
            var task = new TaskCompletionSource<bool>();
            planningPokerClient.Setup(o => o.SubmitEstimation(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<double?>(), It.IsAny<CancellationToken>()))
                .Returns(task.Task);
            var busyIndicatorService = new Mock<IBusyIndicatorService>();
            var busyIndicatorDisposable = new Mock<IDisposable>();
            busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorDisposable.Object);

            var scrumTeam = PlanningPokerData.GetScrumTeam();
            var target = CreateController(planningPokerClient: planningPokerClient.Object, busyIndicator: busyIndicatorService.Object);

            await target.InitializeTeam(scrumTeam, PlanningPokerData.ScrumMasterName);
            var message = new Message { Id = 1, Type = MessageType.EstimationStarted };
            target.ProcessMessages(new Message[] { message });

            var result = target.SelectEstimation(5);

            busyIndicatorService.Verify(o => o.Show());
            busyIndicatorDisposable.Verify(o => o.Dispose(), Times.Never());

            task.SetResult(true);
            await result;

            busyIndicatorDisposable.Verify(o => o.Dispose());
        }

        internal static void AssertNoMemberIsEstimating(PlanningPokerController controller, bool skipScrumMaster = false)
        {
            if (!skipScrumMaster)
            {
                Assert.IsFalse(controller.ScrumMaster.Estimating);
            }

            foreach (var member in controller.Members)
            {
                Assert.IsFalse(member.Estimating);
            }

            AssertNoObserverIsEstimating(controller);
        }

        internal static void AssertNoObserverIsEstimating(PlanningPokerController controller)
        {
            foreach (var observer in controller.Observers)
            {
                Assert.IsFalse(observer.Estimating);
            }
        }

        internal static void AssertMemberIsEstimating(PlanningPokerController controller, string memberName, bool isEstimating)
        {
            var memberItem = controller.Members.First(m => m.Name == memberName);
            Assert.AreEqual(isEstimating, memberItem.Estimating);
        }

        private static PlanningPokerController CreateController(
            IPlanningPokerClient planningPokerClient = null,
            IBusyIndicatorService busyIndicator = null,
            IMemberCredentialsStore memberCredentialsStore = null)
        {
            if (planningPokerClient == null)
            {
                var planningPokerClientMock = new Mock<IPlanningPokerClient>();
                planningPokerClient = planningPokerClientMock.Object;
            }

            if (busyIndicator == null)
            {
                var busyIndicatorMock = new Mock<IBusyIndicatorService>();
                busyIndicator = busyIndicatorMock.Object;
            }

            if (memberCredentialsStore == null)
            {
                var memberCredentialsStoreMock = new Mock<IMemberCredentialsStore>();
                memberCredentialsStore = memberCredentialsStoreMock.Object;
            }

            return new PlanningPokerController(planningPokerClient, busyIndicator, memberCredentialsStore);
        }
    }
}
