namespace Euroins.Tools.Validators
{
    #region Usings
    using System.Text.RegularExpressions;
    #endregion

    public class EgnValidator
    {
        
        public record EGNInfo()
        {
            public string EGN { get; set; } = string.Empty;
            public string? Sex { get; set; }
            public DateTime? BirthDate { get; set; }
            public string? Region { get; set; }

            public override string ToString()
            {
                return $"EGN: {EGN}, Sex: {Sex}, BirthDate: {BirthDate:dd.MM.yyyy}, Region: {Region}";
            }
        }

        private readonly int[] EGN_WEIGHTS = { 2, 4, 8, 5, 10, 9, 7, 3, 6 };

        private readonly Dictionary<int, string> EGN_REGIONS = new()
        {
            {43 ,"Blagoevgrad"}, {93,"Burgas"}, {139,"Varna"}, {169,"Veliko Tarnovo"},
            {183,"Vidin"}, {217,"Vraca"}, {233,"Gabrovo"}, {281,"Kardjali"},
            {301,"Kustendil"}, {319,"Lovech"}, {341,"Montana"}, {377,"Pazardjik"},
            {395,"Pernik"}, {435,"Pleven"}, {501,"Plovdiv"}, {527,"Razgrad"},
            {555,"Ruse"}, {575,"Silistra"}, {601,"Sliven"}, {623,"Smolyan"},
            {721,"Sofia - grad"}, {751,"Sofia -region"}, {789,"Stara Zagora"},
            {821,"Dobrich"}, {843,"Targovishte"}, {871,"Haskovo"}, {903,"Shumen"},
            {925,"Jambol"}, {999,"Other/Unknown"}
        };
        

        public bool IsValid(string egn)
        {
            if (!IsValidBirthDate(egn)) return false;

            byte checksum = Convert.ToByte(egn.Substring(9, 1));
            int egnsum = 0;
            for (var i = 0; i < 9; i++)
            {
                egnsum += Convert.ToInt32(egn[i].ToString()) * EGN_WEIGHTS[i];
            }

            int valid_checksum = egnsum % 11;
            if (valid_checksum == 10) valid_checksum = 0;

            return checksum == valid_checksum;
        }

        public EGNInfo Parse(string egn)
        {
            if (string.IsNullOrWhiteSpace(egn) || !IsValid(egn))
            {
                throw new InvalidDataException($"{egn} invalid format");
            }

            var birthDate = GetValidDate(egn.Substring(0, 6));
            var regionCode = Convert.ToInt32(egn.Substring(6, 3));
            var genderDigit = Convert.ToInt32(egn.Substring(8, 1));

            var info = new EGNInfo
            {
                EGN = egn,
                BirthDate = birthDate,
                Sex = genderDigit % 2 == 0 ? "M" : "F"
            };

            int firstRegionNum = 0;
            foreach (var kvp in EGN_REGIONS)
            {
                if (regionCode >= firstRegionNum && regionCode <= kvp.Key)
                {
                    info.Region = kvp.Value;
                    break;
                }
                firstRegionNum = kvp.Key + 1;
            }

            return info;
        }

        private bool IsValidBirthDate(string egn)
        {
            if (string.IsNullOrWhiteSpace(egn) || egn.Length != 10)
                return false;

            if (!Regex.IsMatch(egn, @"^\d{10}$"))
                return false;

            try
            {
                GetValidDate(egn.Substring(0, 6));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private DateTime GetValidDate(string subEgn)
        {
            int year = Convert.ToInt32(subEgn.Substring(0, 2));
            int month = Convert.ToInt32(subEgn.Substring(2, 2));
            int day = Convert.ToInt32(subEgn.Substring(4, 2));

            if (month > 40)
            {
                month -= 40;
                year += 2000;
            }
            else if (month > 20)
            {
                month -= 20;
                year += 1800;
            }
            else
            {
                year += 1900;
            }

            return DateTime.ParseExact($"{day:D2}/{month:D2}/{year}", "dd/MM/yyyy", null);
        }

       
        public List<string> GenerateAll(DateTime birthDate, string sex = "M", string? regionName = null)
        {
            int year = birthDate.Year;
            int month = birthDate.Month;
            int day = birthDate.Day;

            if (year >= 2000)
            {
                month += 40;
                year -= 2000;
            }
            else if (year < 1900)
            {
                month += 20;
                year -= 1800;
            }
            else
            {
                year -= 1900;
            }

            string datePart = $"{year:D2}{month:D2}{day:D2}";

            int start = 0, end = 999;
            if (!string.IsNullOrWhiteSpace(regionName))
            {
                string normalizedInput = regionName.Trim().ToLower();

                int previousKey = 0;
                foreach (var entry in EGN_REGIONS.OrderBy(kvp => kvp.Key))
                {
                    string normalizedRegion = entry.Value.ToLower();
                    if (normalizedRegion == normalizedInput)
                    {
                        start = previousKey;
                        end = entry.Key;
                        break;
                    }
                    previousKey = entry.Key + 1;
                }
            }

            var result = new List<string>();

            for (int birthOrder = start; birthOrder <= end; birthOrder++)
            {
                int genderDigit = birthOrder % 10;
                if ((sex == "M" && genderDigit % 2 != 0) || (sex == "F" && genderDigit % 2 == 0))
                    continue;

                string regionStr = birthOrder.ToString("D3");
                string firstNine = datePart + regionStr;

                int sum = 0;
                for (int i = 0; i < 9; i++)
                {
                    sum += int.Parse(firstNine[i].ToString()) * EGN_WEIGHTS[i];
                }

                int remainder = sum % 11;
                int checksum = (remainder == 10) ? 0 : remainder;

                result.Add(firstNine + checksum.ToString());
            }

            return result;
        }

    }
}
