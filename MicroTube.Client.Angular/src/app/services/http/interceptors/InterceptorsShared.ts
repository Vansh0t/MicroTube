import { HttpContextToken } from "@angular/common/http";

export const IS_NO_API_REQUEST = new HttpContextToken<boolean>(() => false);
