using System;
using System.Windows.Media;


namespace WpfMultiScreens.Data.Reports
{
    public class JournalsTableItemDto
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string CarriageName { get; set; }
        public string EquipmentName { get; set; }
        public string TrainName { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public DateTime OrderDate { get; set; }
        public bool HasInspection { get; set; }
        public virtual string AuthorName
        {
            get
            {
                string firstName = null, lastName = null, middleName = null;

                if (String.IsNullOrEmpty(Author))
                    return Author;

                var nameParts = Author.Split(' ');

                if (nameParts[0] != null)
                    firstName = nameParts[0];
                
                if (nameParts[1] != null)
                {
                    lastName = nameParts[1];
                    if (lastName.Contains(".") == false && lastName.Length > 3)
                    {
                        lastName = lastName.Substring(0, 1).ToUpper() + ".";
                    }
                }

                if (nameParts[2] != null)
                {
                    middleName = nameParts[2];
                    if (middleName.Contains(".") == false && middleName.Length > 3)
                    {
                        middleName = middleName.Substring(0, 1).ToUpper() + ".";
                    }
                }

                return $"{firstName} {lastName} {middleName}";
            }
        }
    }
}