namespace ASM_SIMS.Helpers
{
    public class UploadFile
    {
        private readonly IFormFile _formFile;
        public UploadFile(IFormFile file)
        {
            _formFile = file;
        }

        public string Upload(string type)
        {
            string uniqueFileName;

            try
            {
                string pathUpload;
                if (type.Equals("images"))
                {
                    pathUpload = "wwwroot\\SIMS\\uploads\\images";
                }
                else
                {
                    pathUpload = "wwwroot\\SIMS\\uploads\\videos";
                }
                string fileName = _formFile.FileName;
                fileName = Path.GetFileName(fileName);
                string uniqueStr = Guid.NewGuid().ToString();
                fileName = uniqueStr + "_" + fileName;
                string pathUploadServer = Path.Combine(Directory.GetCurrentDirectory(), pathUpload, fileName);
                var stream = new FileStream(pathUploadServer, FileMode.Create);
                _formFile.CopyToAsync(stream);
                uniqueFileName = fileName;

            }
            catch (Exception ex)
            {
                uniqueFileName = ex.Message.ToString();
            }

            return uniqueFileName;
        }
    }
}
