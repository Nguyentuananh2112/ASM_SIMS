using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.Validations
{
    // cho phep upload file co kich thuoc nho hon 1MB
    public class AllowedSizeFile : ValidationAttribute
    {
        private readonly int _maxSizeFile;
        public AllowedSizeFile(int sizeFile)
        {
            _maxSizeFile = sizeFile;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxSizeFile)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;

        }
        private string GetErrorMessage()
        {
            return $"File size must be less than {_maxSizeFile} bytes";
        }
    }
}
