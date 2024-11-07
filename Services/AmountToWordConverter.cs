public class AmountToWordsConverter
{
    // Arrays for numbers to words conversion
    private static readonly string[] Ones = { "", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN" };
    private static readonly string[] Tens = { "", "", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };
    private static readonly string[] ThousandsGroups = { "", "THOUSAND", "MILLION", "BILLION" };

    // Main method to convert a decimal amount to words
    public static string ConvertAmountToWords(decimal amount)
    {
        // Split into Ringgit and Cents
        int ringgit = (int)Math.Floor(amount); // Get the whole number (Ringgit)
        int cents = (int)((amount - ringgit) * 100); // Get the decimal part (Cents)

        // Convert Ringgit part
        string ringgitInWords = ConvertToWords(ringgit);

        // Convert Cents part if greater than zero
        string centsInWords = cents > 0 ? $"{ConvertToWords(cents)} CENTS" : "ONLY";

        // Final string in words
        string result = $"RINGGIT MALAYSIA {ringgitInWords}";

        if (cents > 0)
        {
            result += $" AND {centsInWords}" + " ONLY";
        }
        else
        {
            result += $" {centsInWords}";
        }

        return result;
    }

    // Helper method to convert an integer into words
    private static string ConvertToWords(int number)
    {
        if (number == 0)
            return "ZERO";

        int[] numArray = new int[4]; // Store groups of thousands
        int numIndex = 0;

        // Split number into groups of thousands
        while (number > 0)
        {
            numArray[numIndex++] = number % 1000;
            number /= 1000;
        }

        string words = "";
        for (int i = numIndex - 1; i >= 0; i--)
        {
            if (numArray[i] > 0)
            {
                words += ConvertThreeDigitNumberToWords(numArray[i]) + " " + ThousandsGroups[i] + " ";
            }
        }

        return words.Trim();
    }

    // Helper method to convert a three-digit number into words
    private static string ConvertThreeDigitNumberToWords(int number)
    {
        string words = "";

        // Hundreds place
        if (number >= 100)
        {
            words += Ones[number / 100] + " HUNDRED ";
            number %= 100;
        }

        // Tens and ones place
        if (number >= 20)
        {
            words += Tens[number / 10] + " ";
            number %= 10;
        }

        // Ones place (including special cases for numbers between 1 and 19)
        if (number > 0)
        {
            words += Ones[number] + " ";
        }

        return words.Trim();
    }
}