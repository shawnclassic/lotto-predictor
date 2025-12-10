namespace PredictLottoNZ.Models.DTOs;

public class LottoDrawDto
{
    public int Draw { get; set; }
    public DateTime Date { get; set; }
    public int[] WinningNumbers { get; set; } = Array.Empty<int>();
    public int BonusNumber { get; set; }
    public int Powerball { get; set; }
    
    public static LottoDrawDto FromEntity(LottoDraw draw)
    {
        return new LottoDrawDto
        {
            Draw = draw.Draw,
            Date = draw.Date,
            WinningNumbers = new[] 
            { 
                draw.WinningNumber1, 
                draw.WinningNumber2, 
                draw.WinningNumber3, 
                draw.WinningNumber4, 
                draw.WinningNumber5, 
                draw.WinningNumber6 
            },
            BonusNumber = draw.BonusNumber,
            Powerball = draw.Powerball
        };
    }
}