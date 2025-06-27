import { Injectable } from "@angular/core";
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { LoggingService } from "./log.service";

export enum OrientationType {
    Unknown = 'Unknown',
    Portrait = 'Portrait',
    Landscape = 'Landscape',
}

export enum DeviceType {
    Unknown = 'Unknown',
    Tablet = 'Tablet',
    Handset = 'Handset', //Mobile
    Web = 'Web',
}

export enum ScreenSizeType {
    Unknown = 'Unknown',
    XSmall = 'XSmall',
    Small = 'Small',
    Medium = 'Medium',
    Large = 'Large',
    XLarge = 'XLarge',
}

export enum BreakpointType {
    Unknown = 'Unknown',
    HandsetPortrait = 'HandsetPortrait',
    HandsetLandscape = 'HandsetLandscape',
    TabletPortrait = 'TabletPortrait',
    TabletLandscape = 'TabletLandscape',
    WebPortrait = 'WebPortrait',
    WebLandscape = 'WebLandscape',
}


@Injectable({
    providedIn: 'root'
})
export class ResponsiveService {

    private _orientation = OrientationType.Unknown;
    private _deviceType = DeviceType.Unknown;
    private _screenSize = ScreenSizeType.Unknown;

    constructor(private log: LoggingService, private breakpointObserver: BreakpointObserver) {
        this.update();
    }

    public update(): void {
        this.checkScreenSize(this.breakpointObserver);
        this.checkDeviceTypeAndOrientation(this.breakpointObserver);
    }

    public get orientation(): OrientationType {
        return this._orientation;
    }

    public orientationPortrait(): boolean {
        return this._orientation === OrientationType.Portrait;
    }

    public orientationLandscape(): boolean {
        return this._orientation === OrientationType.Landscape;
    }

    public get deviceType(): DeviceType {
        return this._deviceType;
    }

    public deviceDesktop(): boolean {
        return this._deviceType === DeviceType.Web;
    }

    public deviceTablet(): boolean {
        return this._deviceType === DeviceType.Tablet;
    }

    public deviceMobile(): boolean {
        return this._deviceType === DeviceType.Handset;
    }

    public get screenSize(): ScreenSizeType {
        return this._screenSize;
    }

    // Map to associate each breakpoint with a device type and orientation
    private readonly deviceAndOrientation = new Map([
        [Breakpoints.HandsetLandscape, BreakpointType.HandsetLandscape],
        [Breakpoints.HandsetPortrait, BreakpointType.HandsetPortrait],
        [Breakpoints.TabletLandscape, BreakpointType.TabletLandscape],
        [Breakpoints.TabletPortrait, BreakpointType.TabletPortrait],
        [Breakpoints.WebLandscape, BreakpointType.WebLandscape],
        [Breakpoints.WebPortrait, BreakpointType.WebPortrait],
    ]);

    private readonly screenSizeBreakpoints = new Map([
        [Breakpoints.XSmall, ScreenSizeType.XSmall],
        [Breakpoints.Small, ScreenSizeType.Small],
        [Breakpoints.Medium, ScreenSizeType.Medium],
        [Breakpoints.Large, ScreenSizeType.Large],
        [Breakpoints.XLarge, ScreenSizeType.XLarge],
    ]);

    /**
     * Observes changes in the screen size based on predefined breakpoints.
     * Updates the `_screenSize` property whenever a breakpoint is hit.
     *
     * @param {BreakpointObserver} breakpointObserver - The BreakpointObserver instance used to observe screen size changes.
     */
    private checkScreenSize(breakpointObserver: BreakpointObserver): void {
        // Start observing the predefined breakpoints
        breakpointObserver
            .observe([
                Breakpoints.XSmall,
                Breakpoints.Small,
                Breakpoints.Medium,
                Breakpoints.Large,
                Breakpoints.XLarge,
            ])
            .subscribe((result: any) => {
                this.log.info("BreakpointObserver Update:", result)
                // Loop through the breakpoints
                for (const query of Object.keys(result.breakpoints)) {
                    // If the current breakpoint matches the screen size
                    if (result.breakpoints[query]) {
                        // Update the `_screenSize` property with the corresponding ScreenSizeType value
                        // If the breakpoint is not found in the `screenSizeBreakpoints` map, set `_screenSize` to `ScreenSizeType.Unknown`
                        this._screenSize =
                            this.screenSizeBreakpoints.get(query) ??
                            ScreenSizeType.Unknown;
                    }
                }
            });
    }

    /**
     * Observes changes in the screen size based on predefined breakpoints.
     * Updates the `_deviceType` and `_orientation` properties whenever a breakpoint is hit.
     *
     * @param {BreakpointObserver} breakpointObserver - The BreakpointObserver instance used to observe screen size changes.
     */
    private checkDeviceTypeAndOrientation(breakpointObserver: BreakpointObserver): void {
        // Start observing the predefined breakpoints
        breakpointObserver
            .observe([
                Breakpoints.HandsetLandscape,
                Breakpoints.HandsetPortrait,
                Breakpoints.WebLandscape,
                Breakpoints.WebPortrait,
                Breakpoints.TabletLandscape,
                Breakpoints.TabletPortrait,
            ])
            .subscribe((result: any) => {
                // Get the keys of the OrientationType and DeviceType enums
                let orientationTypes = Object.keys(OrientationType).map((key) => key);
                let deviceTypes = Object.keys(DeviceType).map((key) => key);

                // Loop through the breakpoints
                for (const query of Object.keys(result.breakpoints)) {
                    // If the current breakpoint matches the screen size
                    if (result.breakpoints[query]) {
                        // Get the corresponding device type and orientation from the `deviceAndOrientation` map
                        let type = this.deviceAndOrientation.get(query) ?? BreakpointType.Unknown;

                        // Update the `_orientation` property if the current type includes an orientation
                        orientationTypes.forEach((element) => {
                            if (type.indexOf(element) !== -1)
                                this._orientation = element as OrientationType;
                        });

                        // Update the `_deviceType` property if the current type includes a device type
                        deviceTypes.forEach((element) => {
                            if (type.indexOf(element) !== -1)
                                this._deviceType = element as DeviceType;
                        });
                    }
                }
            });
    }

}
