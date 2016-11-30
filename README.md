WWCP RheinEnergie
=================

Usage:

```csharp
var RheinEnergieRN   = new RoamingNetwork(RoamingNetwork_Id.Parse("RheinEnergie"));
var RheinEnergieCSO  = RheinEnergieRN.CreateNewChargingStationOperator(
                           ChargingStationOperator_Id.Parse(Country.Germany, "REK"),
                           I18NString.Create(Languages.de, "RheinEnergie AG"),
                           Configurator: cso => {
                               cso.Homepage = "http://www.rheinenergie.com/TankEn";
                           });

RheinEnergieAPI.ParseRheinEnergieXML(XDocument.Load("Ladestationen_2016-11-29.xml").Root, RheinEnergieCSO);

var GeoJSON = RheinEnergieCSO.ChargingPools.ToFeatureCollection().ToString();
```    
            
