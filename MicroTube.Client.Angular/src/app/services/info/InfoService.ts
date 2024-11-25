import { Injectable } from "@angular/core";
import { MatDialog } from "@angular/material/dialog";
import { InfoPopupDialogComponent } from "../../utility-components/info-popup-dialog/info-popup-dialog.component";

@Injectable({
  providedIn: "root"
})
export class InfoService
{
  private readonly dialog: MatDialog;
  constructor(dialog: MatDialog)
  {
    this.dialog = dialog;
  }
  showAbout()
  {
    this.dialog.open(InfoPopupDialogComponent, {
      data:
      {
        info: `MicroTube is a non-commercial lightweight video hosting service.
        This website is a preview development version of the service, provided as is, without warranty or liability. <br/>
        Project's source: <a href=https://github.com/Vansh0t/MicroTube>github</a>.<br/>
        Contact: <a href=mailto:info@microtube.dev>info@microtube.dev`
      }
    });
  }
  showPrivacy()
  {
    this.dialog.open(InfoPopupDialogComponent, {
      data:
      {
        info: `Below is a summary of how Microtube handles your data:</br></br>

              <b>&#x2022; Data Collection:</b> We collect and store user IP addresses strictly for security and spam protection purposes.</br></br>

              <b>&#x2022; Authentication Information:</b> We securely store authentication credentials (login and password) using industry-standard encryption methods.</br></br>
              
              <b>&#x2022; Data Sharing:</b> We do not share, sell, or distribute any personal information or data with third parties.</br></br>
              
              <b>&#x2022; Liability:</b> As this is a non-commercial development version of the platform, we strive to protect your data; however, microtube.dev is not liable for any potential data leaks or breaches that may occur.</br></br>

              By using this service, you acknowledge and agree to this Privacy Policy.`
      }
    });
  }
  showContentPolicy()
  {
    this.dialog.open(InfoPopupDialogComponent, {
      data:
      {
        info: `At Microtube, we aim to create a positive and safe environment for all users. Please adhere to the following content uploading guidelines:</br></br>

               <b>&#x2022; No NSFW Content:</b> We enforce a strict no NSFW (Not Safe For Work) policy. Any content that violates this rule will be removed immediately.</br></br>

               <b>&#x2022; Content Removal:</b> As this is a development version, Microtube.dev reserves the right to remove any content at any time without prior warning or notification.</br></br>

               <b>&#x2022; File Size:</b> As this is a development version, the processing times could be long for big files. It is not recommended to upload big files.</br></br>

               By using this service, you agree to follow these content guidelines.`
      }
    });
  }
}
