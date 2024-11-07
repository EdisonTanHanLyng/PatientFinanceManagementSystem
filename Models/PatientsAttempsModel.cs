using PFMS_MI04.Services;
using System.Text.Json;

namespace PFMS_MI04.Models
{
    public class PatientsAttempsModel
    {
        public string ID {  get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }

        public string DataReturn()
        {
            return $"{ID} , {Name} , {Status}, {Date}";
        }
    }
}
