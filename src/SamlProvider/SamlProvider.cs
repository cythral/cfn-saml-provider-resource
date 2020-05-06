using System.IO;
using System;
using System.Threading.Tasks;

using Amazon.S3.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;

using Cythral.CloudFormation.Resources.Aws;

using Cythral.CloudFormation.CustomResource.Attributes;
using Cythral.CloudFormation.CustomResource.Core;

namespace Cythral.CloudFormation.Resources
{
    [CustomResource]
    public partial class SamlProvider
    {
        public class Properties
        {
            public string? CreatorRoleArn { get; set; } = null;

            public string? DownloaderRoleArn { get; set; } = null;

            public string Name { get; set; } = "";

            public string SamlMetadataDocumentLocation { get; set; } = "";
        }

        private IamFactory iamFactory = new IamFactory();
        private S3GetObjectFacadeFactory s3GetObjectFacadeFactory = new S3GetObjectFacadeFactory();

        public async Task<Response> Create()
        {
            var props = this.Request.ResourceProperties;
            var iamClient = await iamFactory.Create(props.CreatorRoleArn);
            var s3GetObjectFacade = s3GetObjectFacadeFactory.Create();

            var metadataDoc = await s3GetObjectFacade.GetObject(props.SamlMetadataDocumentLocation, props.DownloaderRoleArn);
            var createSamlProviderResponse = await iamClient.CreateSAMLProviderAsync(new CreateSAMLProviderRequest
            {
                Name = props.Name,
                SAMLMetadataDocument = metadataDoc
            });

            return new Response
            {
                PhysicalResourceId = createSamlProviderResponse.SAMLProviderArn
            };
        }

        public async Task<Response> Update()
        {
            var props = this.Request.ResourceProperties;
            var iamClient = await iamFactory.Create(props.CreatorRoleArn);

            return await Task.FromResult(new Response
            {

            });
        }

        public async Task<Response> Delete()
        {
            return await Task.FromResult(new Response
            {

            });
        }
    }
}
