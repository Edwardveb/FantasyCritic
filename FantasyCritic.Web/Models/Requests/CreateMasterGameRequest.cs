using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Requests
{
    public class CreateMasterGameRequest
    {
        [Required]
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public LocalDate? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        [Required]
        public int MinimumReleaseYear { get; set; }
        [Required]
        public int EligibilityLevel { get; set; }
        [Required]
        public bool YearlyInstallment { get; set; }
        [Required]
        public bool EarlyAccess { get; set; }
        [Required]
        public string BoxartFileName { get; set; }

        public MasterGame ToDomain(EligibilityLevel eligibilityLevel)
        {
            MasterGame masterGame = new MasterGame(Guid.NewGuid(), GameName, EstimatedReleaseDate, ReleaseDate, OpenCriticID, null, MinimumReleaseYear, eligibilityLevel, YearlyInstallment, EarlyAccess, BoxartFileName);
            return masterGame;
        }
    }
}
