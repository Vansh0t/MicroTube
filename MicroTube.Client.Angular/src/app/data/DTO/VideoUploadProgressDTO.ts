export interface VideoUploadProgressDTO
{
  id: string,
  title: string;
  description: string | null;
  status: string;
  message: string | null;
  lengthSeconds: number | null,
  lengthHuman: string | null;
  timestamp: string;
  timestampHuman: string | null;
}
