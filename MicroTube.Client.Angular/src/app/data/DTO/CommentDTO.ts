import { DateTime } from "luxon";
export class CommentDto
{
  id: string;
  userId: string;
  userAlias: string | null;
  videoId: string;
  content: string;
  time: DateTime;
  constructor(raw: CommentRawDto)
  {
    this.id = raw.id;
    this.userId = raw.userId;
    this.userAlias = raw.userAlias;
    this.videoId = raw.videoId;
    this.content = raw.content;
    this.time = DateTime.fromISO(raw.time);
  }
}
export interface CommentRawDto
{
  id: string;
  userId: string;
  userAlias: string | null;
  videoId: string;
  content: string;
  time: string;
}
