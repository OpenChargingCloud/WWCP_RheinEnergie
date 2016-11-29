/*
 * Copyright (c) 2010-2016, Achim 'ahzf' Friedland <achim.friedland@graphdefined.com>
 * This file is part of the Open Charging Cloud API <https://github.com/OpenChargingCloud/WWCP_RheinEnergie>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Aegir;

#endregion

namespace org.GraphDefined.WWCP.ExternalAPIs.RheinEnergie
{

    public static class RheinEnergieAPI
    {

        public static void ParseRheinEnergieXML(XElement                 XML,
                                                ChargingStationOperator  Operator)
        {

            #region Documentation

            // <ladestationen>
            //
            //   <standort name                  = "TankE - Kreishaus Heidkamp"
            //             kurzbezeichnung       = "KHH01"
            //             strasse               = "Am Rübezahlwald "
            //             hausnr                = "7"
            //             plz                   = "51469"
            //             Ort                   = "Bergisch Gladbach "
            //             kreis                 = "Rheinisch-Bergischer Kreis"
            //             land                  = "DE"
            //             latitude              = "50.97695"
            //             longitude             = "7.143299"
            //             standortbeschreibung  = "Die Ladestation befindet sich auf dem Parkplatz des Kreishauses."
            //             anfahrtsbeschreibung  = "Folgen Sie den Hinweisschildern Richtung Abflug Terminal 2. Die Zufahrt erfolgt über den Kurzzeitparkplatz durch die Schranken vor Terminal 2 (10 Minuten Kulanzzeit für Durchfahrt). Die TankE befindet sich auf dem Dach von Parkhaus 2. Die Zufahrt durch Parkhaus 2 ist allerdings nicht möglich."
            //             kommentar             = "Registrierung nötig unter www.rheinenergie.com/TankEn-Registrierung"
            //             webseite              = "http://www.rheinenergie.com/TankEn"
            //             betreiber             = "RheinEnergie AG"
            //             betreiberid           = "REK"
            //             partner               = "Rheinisch-Bergischer-Kreis"
            //             stoerhotline          = "0221 34645600"
            //             zweiviersieben        = "true">
            //
            //    <station evse_ID        = "DEREKE00082"         == "DE*REK*E00082"
            //             ID             = "99998"
            //             status         = "In Betrieb"
            //             inbetriebnahme = "29.10.2015"
            //             fahrzeugklasse = "Vierrad"
            //             gelaende       = "Parkplatz"
            //             gebaeudeebene  = "Ebene 6"
            //             deckenhoehe    = "190"
            //             parkgebuehren  = "kostenlos"
            //             hersteller     = "EBG complEo GmbH"
            //             montageart     = "Ladesäule freistehend"
            //             oekostrom      = "true">
            //
            //      <ladepunkte anz="2">
            //
            //        <ladepunkt evse_ID = "DEREKE00082001"       == "DE*REK*E00082*001"
            //                   lage    = "rechts"
            //                   access  = "Halböffentlich"
            //                   sms_id  = "82R">
            //
            //          <stecker steckerart   = "Typ 2"
            //                   amp          = "32"
            //                   volt         = "400"
            //                   phases       = "AC3"
            //                   max_leistung = "22"
            //                   festeskabel  = "false"
            //                   verriegelung = "true" />
            //
            //          <zugang  zugangart_1 = "RFID"
            //                   zugangart_2 = "SMS" />
            //
            //        </ladepunkt>
            //
            //        <ladepunkt evse_ID = "DEREKE00082002"       == "DE*REK*E00082*002"
            //                   lage    = "links"
            //                   access  = "Halböffentlich"
            //                   sms_id  = "82L">
            //
            //          <stecker steckerart   = "Typ 2"
            //                   amp          = "32"
            //                   volt         = "400"
            //                   phases       = "AC3"
            //                   max_leistung = "22"
            //                   festeskabel  = "false"
            //                   verriegelung = "true" />
            //
            //          <zugang  zugangart_1 = "RFID"
            //                   zugangart_2 = "SMS" />
            //
            //        </ladepunkt>
            //
            //      </ladepunkte>
            //    </station>
            //
            //   </standort>
            //
            // </ladestationen>

            #endregion

            ChargingPool pool;
            ChargingStation  station;
            EVSE             evse;

            foreach (var Standort in XML.Elements("standort"))
            {

                #region Create new charging pool

                pool = Operator.CreateChargingPool(ChargingPool_Id.Parse(Operator.Id, Standort.Attribute("kurzbezeichnung").Value.Trim()),
                                                   Configurator: newpool => {

                                                       newpool.BrandName            = Standort.MapAttributeValueOrDefault("partner",
                                                                                                                          value => I18NString.Create(Languages.de, value.Trim()));

                                                       newpool.Description          = Standort.MapAttributeValueOrDefault("standortbeschreibung",
                                                                                                                          value => I18NString.Create(Languages.de, value.Trim()));

                                                       newpool.ArrivalInstructions  = Standort.MapAttributeValueOrDefault("anfahrtsbeschreibung",
                                                                                                                          value => I18NString.Create(Languages.de, value.Trim()));

                                                       newpool.GeoLocation          = GeoCoordinate.Create(
                                                                                          Latitude. Parse(Standort.Attribute("latitude"). Value.Trim()),
                                                                                          Longitude.Parse(Standort.Attribute("longitude").Value.Trim())
                                                                                      );

                                                       newpool.Address              = Address.Create(
                                                                                          Standort.MapAttributeValueOrDefault("land",    value => Country.Parse(value.Trim())),
                                                                                          Standort.MapAttributeValueOrDefault("plz",     value => value.Trim()),
                                                                                          Standort.MapAttributeValueOrDefault("Ort",     value => I18NString.Create(Languages.de, value.Trim())),
                                                                                          Standort.MapAttributeValueOrDefault("strasse", value => value.Trim()),
                                                                                          Standort.MapAttributeValueOrDefault("hausnr",  value => value.Trim())
                                                                                      );

                                                       newpool.OpeningTimes         = Standort.MapAttributeValueOrDefault("zweiviersieben",
                                                                                                                          value => value == "true"
                                                                                                                              ? OpeningTimes.Open24Hours
                                                                                                                              : null);

                                                      });

                #endregion

                foreach (var Station in Standort.Elements("station"))
                {

                    #region Create new charging station

                    station = pool.CreateChargingStation(ChargingStation_Id.Parse(Operator.Id, Station.Attribute("evse_ID").Value));

                    #endregion

                    foreach (var Ladepunkt in Station.Element("ladepunkte").Elements("ladepunkt"))
                    {

                        #region Create new EVSE

                        evse = station.CreateEVSE(EVSE_Id.Parse(Operator.Id, Ladepunkt.Attribute("evse_ID").Value));

                        var zugänge = Ladepunkt.Element("zugang") != null

                                          ? Ladepunkt.Element("zugang").Attributes().
                                                Where (attr => attr.Name.LocalName.StartsWith("zugangart_", StringComparison.Ordinal)).
                                                Select(attr => attr.Value).ToArray()

                                          : new String[0];

                        //foreach (var Stecker in Ladepunkt.Elements("stecker"))
                        //{

                        //    var socket = evse.C(EVSE_Id.Parse(Operator.Id, Ladepunkt.Attribute("evse_ID").Value));

                        //}

                        #endregion

                    }

                }

            }

        }

    }

}
