namespace RiseBot.Results
{
    public abstract class BaseLottoResult
    {
        public string ClanName { get; set; }
        public string ClanTag { get; set; }

        public string OpponentName { get; set; }
        public string OpponentTag { get; set; }

        public string WarLogComparison { get; set; }
    }

    public class LottoFailed : BaseLottoResult
    {
        public string Reason { get; set; }
    }

    public class LottoDraw : BaseLottoResult
    {
        public string HighSyncWinnerTag { get; set; }
    }

    public class LottoResult : BaseLottoResult
    {
        public bool ClanWin { get; set; }
    }
}
