using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FluidCash.Helpers.ObjectFormatters.DTOs.Responses;
using FluidCash.Helpers.ObjectFormatters.ObjectWrapper;
using FluidCash.IExternalServicesRepo;
using System.Text.RegularExpressions;

namespace FluidCash.ExternalServicesRepo;

public class CloudinaryServices : ICloudinaryServices
{
    Cloudinary _cloudinaryServices;

    public CloudinaryServices(Cloudinary cloudinaryServices)
    {
        _cloudinaryServices = cloudinaryServices;
    }

    public async Task<StandardResponse<DocumentUploadResponse>>
        UploadFileToCloudinaryAsync
        (IFormFile fileForUpload)
    {
        var listOfImageExtensions = new List<string>() { ".jpg", ".png", ".jpeg" };
        var listOfVideoExtensions = new List<string>() { ".mpeg", ".mpg", ".3gp", ".mp4" };
        var rawUploadResult = new RawUploadResult();
        var imageUploadResult = new ImageUploadResult();
        var videosUploadResult = new VideoUploadResult();
        string folderName = "PayBigi";
        string filePathUrl = string.Empty;
        var publicId = string.Empty;
        string errorOccured = string.Empty;

        using var stream = fileForUpload.OpenReadStream();
        var cloudFileName = string.Concat(Path.GetFileNameWithoutExtension(fileForUpload.FileName), Ulid.NewUlid().ToString().Substring(1, 7));
        //Remove spaces and special characters to match cloudinary requirement for PublicId and easy retrieval of PublicId from uri
        cloudFileName = Regex.Replace(cloudFileName, @"[\W_]", "");
        if (listOfImageExtensions.Any(ext => fileForUpload.FileName.ToLower().EndsWith(ext)))
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(cloudFileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = folderName,
                PublicId = cloudFileName
            };

            imageUploadResult = await _cloudinaryServices.UploadAsync(uploadParams);
            if (imageUploadResult.Error != null)
            {
                errorOccured = $"Failed to upload {fileForUpload.FileName}. Details {imageUploadResult.Error.Message}";
                return StandardResponse<DocumentUploadResponse>.Failed(data: null, errorMessage: errorOccured);
            }
            else
            {
                filePathUrl = imageUploadResult.Uri.AbsoluteUri;
                publicId = imageUploadResult.PublicId;
            }
        }
        else if (listOfVideoExtensions.Any(ext => fileForUpload.FileName.ToLower().EndsWith(ext)))
        {
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(cloudFileName, stream),
                Folder = folderName,
                PublicId = cloudFileName,
            };
            videosUploadResult = await _cloudinaryServices.UploadAsync(uploadParams);
            if (videosUploadResult.Error != null)
            {
                errorOccured = $"Failed to upload {fileForUpload.FileName}. Details {videosUploadResult.Error.Message}";
                return StandardResponse<DocumentUploadResponse>.Failed(data: null, errorMessage: errorOccured);
            }
            else
            {
                filePathUrl = videosUploadResult.Uri.AbsoluteUri;
                publicId = videosUploadResult.PublicId;
            }
        }
        else
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(cloudFileName, stream),
                Folder = folderName,
                PublicId = cloudFileName,
            };
            rawUploadResult = await _cloudinaryServices.UploadAsync(uploadParams);
            if (rawUploadResult.Error != null)
            {
                errorOccured = $"Failed to upload {fileForUpload.FileName}. Details {rawUploadResult.Error.Message}";
                return StandardResponse<DocumentUploadResponse>.Failed(data: null, errorMessage: errorOccured);
            }
            else
            {
                filePathUrl = rawUploadResult.Uri.AbsoluteUri;
                publicId = rawUploadResult.PublicId;
            }
        }
        var response = new DocumentUploadResponse
            (
                fileUrlPath: filePathUrl,
                filePublicId: publicId
            );

        return StandardResponse<DocumentUploadResponse>.Success(data: response);
    }

    public async Task<StandardResponse<string>>
        DeleteFileFromCloudinaryAsync
        (string filePublicId)
    {

        var result = await _cloudinaryServices.DestroyAsync(new DeletionParams(filePublicId));
        if (!result.Result.Equals("ok"))
        {
            var errorMsg = "An error occured deleting file. Details: " + result.Result;
            return StandardResponse<string>.Failed(data: null, errorMessage: errorMsg);
        }
        var successMsg = "File Successfully Deleted";
        return StandardResponse<string>.Success(successMsg);
    }
}
