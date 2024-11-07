namespace PFMS_MI04.Models
{
    public class BackupList
    {
        public string[] types { get; }
        public List<BackupDetails> Items { get; set; }
        public List<BackupDetails> Items_2 { get; set; }
        public double[] typeCount { get; }
        public double[] percentages { get; }

        private static DateTime now = DateTime.Now;
        private static DateTime sixMonthsAgo = now.AddMonths(-6);

        //public func
        public BackupList()
        {
            types = ["Full", "Account", "Reminders", "Documents", "Security Logging", "Backups", "Pricings"];
            Items = new List<BackupDetails>();
            Items_2 = new List<BackupDetails>();
            typeCount = [0, 0, 0, 0, 0, 0, 0];
            percentages = [0, 0, 0, 0, 0, 0, 0];
        }

        public void refresh()
        {
            Items.Clear();
            Items_2.Clear();
            for (int i = 0; i < typeCount.Length; i++)
            {
                typeCount[i] = 0;
            }
            for (int i = 0; i < percentages.Length; i++)
            {
                percentages[i] = 0;
            }
        }

        public void getCount()
        {
            Items_2 = Items.Where(item => item.date >= sixMonthsAgo && item.date <= now).ToList();
            Items_2.ForEach(item => addToCount(item));

            toPercentage();
            sortList();
        }

        //private func
        private void sortList ()
        {
            Items = Items.OrderByDescending(item => item.date).ToList();
        }

        private void addToCount(BackupDetails newItem)
        {
            if (newItem.type.Equals(types[0]))
            {
                typeCount[0]++;
            }

            if (newItem.type.Equals(types[1]))
            {
                typeCount[1]++;
            }

            if (newItem.type.Equals(types[2]))
            {
                typeCount[2]++;
            }

            if (newItem.type.Equals(types[3]))
            {
                typeCount[3]++;
            }

            if (newItem.type.Equals(types[4]))
            {
                typeCount[4]++;
            }

            if (newItem.type.Equals(types[5]))
            {
                typeCount[5]++;
            }

            if (newItem.type.Equals(types[6]))
            {
                typeCount[6]++;
            }
        }

        private void toPercentage()
        {
            for (int i = 0; i < typeCount.Length; i++)
            {
                percentages[i] = (typeCount[i] / Items_2.Count) * 100;
                percentages[i] = Math.Round(percentages[i], 3);
            }
        }
    }
}