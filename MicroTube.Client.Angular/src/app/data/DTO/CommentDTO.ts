import { DateTime } from "luxon";
export class CommentDTO
{
  id: string;
  userId: string;
  userAlias: string | null;
  videoId: string;
  content: string;
  time: DateTime;
  constructor(raw: CommentRawDTO)
  {
    this.id = raw.id;
    this.userId = raw.userId;
    this.userAlias = raw.userAlias;
    this.videoId = raw.videoId;
    this.content = raw.content;
    this.time = DateTime.fromISO(raw.time);
  }
}
export interface CommentRawDTO
{
  id: string;
  userId: string;
  userAlias: string | null;
  videoId: string;
  content: string;
  time: string;
}
