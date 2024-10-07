import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { RouterModule } from "@angular/router";

import { AppComponent } from "./app.component";
import { HomeComponent } from "./home/home.component";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { MatToolbarModule } from "@angular/material/toolbar";
import { MatButtonModule } from "@angular/material/button";
import { MatMenuModule } from "@angular/material/menu";
import { MatInputModule } from "@angular/material/input";
import { AppAuthModule } from "./auth/auth.module";
import { SignUpFormComponent } from "./auth/sign-up-form/sign-up-form.component";
import { MatIconModule } from "@angular/material/icon";
import { AuthManager } from "./services/auth/AuthManager";
import { JWTAuthInterceptor } from "./services/http/interceptors/JWTAuthInterceptor";
import { APIBaseURLInterceptor } from "./services/http/interceptors/APIBaseURLInterceptor";
import { EmailConfirmationCallbackComponent } from "./auth/email-confirmation-callback/email-confirmation-callback.component";
import { PasswordChangeFormComponent } from "./auth/password-change-form/password-change-form.component";
import { UserModule } from "./user/user.module";
import { EmailPasswordProfileComponent } from "./user/email-password-profile/email-password-profile.component";
import { VideoListingModule } from "./video-listing/video-listing.module";
import { VideoWatchComponent } from "./video-listing/video-watch/video-watch.component";
import { VideoUploadComponent } from "./video-listing/video-upload/video-upload.component";
import { UploadProgressListComponent } from "./video-listing/upload-progress-list/upload-progress-list.component";
import { UtilityComponentsModule } from "./utility-components/utility-components.module";
import { MenusModule } from "./menus/menus.module";
export function getBaseUrl()
{
  return document.getElementsByTagName("base")[0].href;
}
const providers = [
  {
    provide: "BASE_URL",
    useFactory: getBaseUrl,
    deps: []
  },
  {
    provide: HTTP_INTERCEPTORS,
    useClass: APIBaseURLInterceptor,
    multi: true,
    deps: []
  },
  {
    provide: HTTP_INTERCEPTORS,
    useClass: JWTAuthInterceptor,
    multi: true,
    deps: [AuthManager]
  }
];

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    HttpClientModule,
    AppAuthModule,
    RouterModule.forRoot([
      { path: "", component: HomeComponent, pathMatch: "full" },
      { path: "watch/:id", component: VideoWatchComponent, pathMatch: "full" },
      { path: "upload", component: VideoUploadComponent, pathMatch: "full" },
      { path: "upload/list", component: UploadProgressListComponent, pathMatch: "full" },
      { path: "signup", component: SignUpFormComponent, pathMatch: "full" },
      { path: "user/profile", component: EmailPasswordProfileComponent, pathMatch: "full" },
      { path: "authentication/emailpassword/confirmemail", component: EmailConfirmationCallbackComponent, pathMatch: "full" },
      { path: "authentication/emailpassword/resetpassword", component: PasswordChangeFormComponent, pathMatch: "full" },
    ]),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule,
    MatInputModule,
    MatIconModule,
    UserModule,
    VideoListingModule,
    UtilityComponentsModule,
    MenusModule
  ],
  providers: providers,
  bootstrap: [AppComponent]
})
export class AppModule { }
