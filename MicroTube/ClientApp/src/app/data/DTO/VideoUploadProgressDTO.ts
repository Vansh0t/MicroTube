export interface VideoUploadProgressDTO
{
  id: string,
  title: string;
  description: string | null;
  status: VideoUploadStatus;
  message: string | null;
}
//public enum VideoUploadStatus {InQueue, InProgress, Fail, Success}
export enum VideoUploadStatus { InQueue, InProgress, Fail, Success }
