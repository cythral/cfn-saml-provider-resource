namespace Cythral.CloudFormation.Resources.Aws
{
    public class S3GetObjectFacadeFactory
    {
        public virtual S3GetObjectFacade Create()
        {
            return new S3GetObjectFacade();
        }
    }
}