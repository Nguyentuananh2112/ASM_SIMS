using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.Validations
{
    public class AllowedTypeFile : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedTypeFile(string[] extensions)
        {
            _extensions = extensions;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }
        private string GetErrorMessage()
        {
            return $"File type is not allowed!";
        }
    }
}
