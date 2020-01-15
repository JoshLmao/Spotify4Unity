# API Overview

Since v1.5.0, S4U is now open source and maintained over at [Github](https://github.com/JoshLmao/Spotify4Unity).

### SpotifyService & MobileSpotifyService

Main entry scripts for handling connecting to Spotify Service. Platform dependant since Mobile uses Implicit auth flow, while PC uses Authorization Code flow. **SpotifyServiceBase** is a shared base class between these two

### Event Manager

Used to broadcast events from the services to **SpotifyUIBase**. A SpotifyService has one **Event Manager** which detects for changes and then broadcasts. To broadcast a new event, create a new class and inherit from "api/events/GameEventBase"

### SpotifyUIBase

Base class for listening to events from the **Event Manager**

### Planned/Potential Improvements

* Remove all custom dto's and use SpotifyAPI.NET classes for holding/transfering data
* If possible, implement Authorization Code auth for mobile