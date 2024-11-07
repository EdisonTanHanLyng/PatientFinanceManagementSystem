namespace PFMS_MI04.Models.Patients
{
    public class NewPatDocData
    {
        public PatientDoc patientDoc { get; set; }
        public IFormFile formFile { get; set; }

        public NewPatDocData()
        {
            patientDoc = new PatientDoc();
            formFile = null;
        }
        
        public NewPatDocData(PatientDoc patientDoc, IFormFile form)
        {
            this.patientDoc = patientDoc;
            formFile = form;
        }
    }
}
