namespace PFMS_MI04.Models
{
    public class MainMenuModel_Patients_Attemps
    {
        public string ID {  get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

        public string dataReturn()
        {
            return $"{ID} , {Name} , {Status}";
        }


    }
}
