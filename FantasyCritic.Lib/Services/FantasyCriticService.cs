using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class FantasyCriticService
    {
        private readonly FantasyCriticUserManager _userManager;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IClock _clock;

        public FantasyCriticService(FantasyCriticUserManager userManager, IFantasyCriticRepo fantasyCriticRepo, IClock clock)
        {
            _userManager = userManager;
            _fantasyCriticRepo = fantasyCriticRepo;
            _clock = clock;
        }

        public async Task<FantasyCriticLeague> CreateLeague(LeagueCreationParameters parameters)
        {
            LeagueOptions newOptions = null;
            LeagueOptions options = new LeagueOptions(parameters);
            FantasyCriticLeague newLeague = new FantasyCriticLeague(Guid.NewGuid(), parameters.LeagueName, parameters.Manager, new List<int>(parameters.InitialYear), options);
            await _fantasyCriticRepo.CreateLeague(newLeague, parameters.InitialYear);
            return newLeague;
        }

        public Task<Maybe<FantasyCriticLeague>> GetLeagueByID(Guid id)
        {
            return _fantasyCriticRepo.GetLeagueByID(id);
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetPlayersInLeague(FantasyCriticLeague league)
        {
            return _fantasyCriticRepo.GetPlayersInLeague(league);
        }

        public async Task<Result> InviteUser(FantasyCriticLeague league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            bool userInvited = await UserIsInvited(league, inviteUser);
            if (userInvited)
            {
                return Result.Fail("User is already invited to this league.");
            }

            await _fantasyCriticRepo.SaveInvite(league, inviteUser);

            return Result.Ok();
        }

        public async Task<Result> AcceptInvite(FantasyCriticLeague league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            bool userInvited = await UserIsInvited(league, inviteUser);
            if (!userInvited)
            {
                return Result.Fail("User is not invited to this league.");
            }

            await _fantasyCriticRepo.AcceptInvite(league, inviteUser);

            return Result.Ok();
        }

        public async Task<Result> DeclineInvite(FantasyCriticLeague league, FantasyCriticUser inviteUser)
        {
            bool userInLeague = await UserIsInLeague(league, inviteUser);
            if (userInLeague)
            {
                return Result.Fail("User is already in league.");
            }

            bool userInvited = await UserIsInvited(league, inviteUser);
            if (!userInvited)
            {
                return Result.Fail("User is not invited to this league.");
            }

            await _fantasyCriticRepo.DeclineInvite(league, inviteUser);

            return Result.Ok();
        }

        public Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(FantasyCriticLeague league)
        {
            return _fantasyCriticRepo.GetOutstandingInvitees(league);
        }

        private async Task<bool> UserIsInLeague(FantasyCriticLeague league, FantasyCriticUser user)
        {
            var playersInLeague = await GetPlayersInLeague(league);
            return playersInLeague.Any(x => x.UserID == user.UserID);
        }

        private async Task<bool> UserIsInvited(FantasyCriticLeague league, FantasyCriticUser inviteUser)
        {
            var playersInvited = await GetOutstandingInvitees(league);
            return playersInvited.Any(x => x.UserID == inviteUser.UserID);
        }

        public  Task<IReadOnlyList<int>> GetOpenYears()
        {
            return _fantasyCriticRepo.GetOpenYears();
        }

        public Task<IReadOnlyList<FantasyCriticLeague>> GetLeaguesForUser(FantasyCriticUser currentUser)
        {
            return _fantasyCriticRepo.GetLeaguesForUser(currentUser);
        }

        public Task<IReadOnlyList<FantasyCriticLeague>> GetLeaguesInvitedTo(FantasyCriticUser currentUser)
        {
            return _fantasyCriticRepo.GetLeaguesInvitedTo(currentUser);
        }

        public Task<IReadOnlyList<MasterGame>> GetMasterGames()
        {
            return _fantasyCriticRepo.GetMasterGames();
        }

        public Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            return _fantasyCriticRepo.GetMasterGame(masterGameID);
        }

        public Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
        {
            return _fantasyCriticRepo.UpdateCriticStats(masterGame, openCriticGame);
        }

        public async Task<Result> ClaimGame(ClaimGameDomainRequest request)
        {
            PlayerGame playerGame = new PlayerGame(request.Year, request.GameName, _clock.GetCurrentInstant(), request.Waiver, request.AntiPick, null, request.MasterGame);

            await _fantasyCriticRepo.AddPlayerGame(request.League, request.User, playerGame);

            return Result.Ok();
        }
    }
}
