import { VgApiService } from "@videogular/ngx-videogular/core";
import { Subscription, interval } from "rxjs";

export class VgPlayerPlaytimeTracker
{
  tickMilliseconds = 50;
  playtime: number = 0;
  private callbacks: PlaytimeTrackerCallback[] = [];
  private readonly vgApi: VgApiService;
  private readonly intervalSubscription: Subscription;
  constructor(vgApi: VgApiService)
  {
    this.vgApi = vgApi;
    this.intervalSubscription = interval(50).subscribe(this.onTick.bind(this));
  }
  onPlaytime(seconds: number, callback: ()=>void, name : string | null = null)
  {
    this.callbacks.push({name: name, seconds: seconds, callback: callback });
  }
  dispose()
  {
    this.intervalSubscription.unsubscribe();
  }
  private onTick()
  {
    if (this.vgApi.state == "playing")
    {
      this.playtime += this.tickMilliseconds / 1000;
      const callbacksToInvoke = this.callbacks.filter(_ => this.playtime >= _.seconds);
      callbacksToInvoke.forEach(_ =>
      {
        _.callback();
      });
      this.callbacks = this.callbacks.filter(_ => this.playtime < _.seconds);
    }
  }

}
type PlaytimeTrackerCallback = {
  name: string|null;
  seconds: number;
  callback: ()=>void
}
