using System;
using System.Collections.Generic;
using System.Text;

namespace OnefinityProbeAutoCalibration.Services
{
    public class UserService : IUserService
    {
        public decimal RequestDistanceFromUser(decimal preSetDistance, string descriptiveText)
        {
            if (preSetDistance > 0)
            {
                return preSetDistance;
            }

            while (true)
            {
                Console.WriteLine(descriptiveText);
                var stockWidthRaw = Console.ReadLine();
                if (decimal.TryParse(stockWidthRaw, out var parsedStockWidth))
                {
                    return parsedStockWidth;
                }

                Console.WriteLine("The value you entered is not a valid number, please try again.");
            }
        }
    }
}
