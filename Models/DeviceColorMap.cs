using System.Collections.Generic;
using System.Diagnostics;

namespace Phonova.Models
{
    public static class DeviceColorMap
    {
        private static readonly Dictionary<string, Dictionary<string, string>> ColorMap = new Dictionary<string, Dictionary<string, string>>
        {
            // iPad Mini 5 WiFi
            { "iPad11,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Mini 5 Cellular
            { "iPad11,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Air 3 Wi-Fi
            { "iPad11,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Air 3 Cellular
            { "iPad11,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad 8 WiFi
            { "iPad11,6", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad 8 Cellular
            { "iPad11,7", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad 9th Generation
            { "iPad12,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad 9th Generation
            { "iPad12,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad Air 4th Gen WiFi
            { "iPad13,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "18", "GREEN" },
                    { "2", "SILVER" },
                    { "3", "ROSE GOLD" },
                    { "4", "SKY BLUE" },
                    { "5", "GREEN" }
                }
            },
            // iPad Pro 5 12.9 inch
            { "iPad13,10", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro 5 12.9 inch
            { "iPad13,11", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Air 5th Gen (Wi-Fi)
            { "iPad13,16", new Dictionary<string, string>
                {
                    { "1", "SPACE GRAY" },
                    { "3", "PINK" },
                    { "4", "BLUE" },
                    { "6", "STARLIGHT" },
                    { "7", "PURPLE" }
                }
            },
            // iPad Air 5th Gen Cellular
            { "iPad13,17", new Dictionary<string, string>
                {
                    { "1", "SPACE GRAY" },
                    { "3", "PINK" },
                    { "4", "BLUE" },
                    { "6", "STARLIGHT" },
                    { "7", "PURPLE" }
                }
            },
            // iPad (10th generation) WiFi
            { "iPad13,18", new Dictionary<string, string>
                {
                    { "1", "SILVER" },
                    { "2", "BLUE" },
                    { "3", "YELLOW" },
                    { "4", "PINK" }
                }
            },
            // iPad (10th generation)
            { "iPad13,19", new Dictionary<string, string>
                {
                    { "1", "SILVER" },
                    { "2", "BLUE" },
                    { "3", "YELLOW" },
                    { "4", "PINK" }
                }
            },
            // iPad Air 4th Gen Cellular
            { "iPad13,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "18", "GREEN" },
                    { "2", "SILVER" },
                    { "3", "ROSE GOLD" },
                    { "4", "SKY BLUE" },
                    { "5", "GREEN" }
                }
            },
            // iPad Pro 5 11 inch
            { "iPad13,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro 5 11inch
            { "iPad13,5", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro 5 11 inch
            { "iPad13,6", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro 11 3rd Gen Cellular
            { "iPad13,7", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro 5 12.9 inch
            { "iPad13,8", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro 5 12.9 inch
            { "iPad13,9", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Mini 6th Gen
            { "iPad14,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "17", "PURPLE" },
                    { "2", "STARLIGHT" },
                    { "3", "PINK" },
                    { "4", "PINK" },
                    { "6", "STARLIGHT" },
                    { "7", "PURPLE" }
                }
            },
            // iPad Air 6th Gen 13 inch
            { "iPad14,10", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "SILVER" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Air 6th Gen 13 inch
            { "iPad14,11", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "SILVER" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Mini 6th Gen
            { "iPad14,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "17", "PURPLE" },
                    { "18", "ALPINE GREEN" },
                    { "2", "STARLIGHT" },
                    { "3", "PINK" },
                    { "4", "PINK" },
                    { "6", "STARLIGHT" },
                    { "7", "PURPLE" }
                }
            },
            // iPad Pro (11-inch) (4th generation)
            { "iPad14,3", new Dictionary<string, string>
                {
                    { "1", "GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro (11-inch) (4th generation) Cellular
            { "iPad14,4", new Dictionary<string, string>
                {
                    { "1", "GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro (12.9-inch) (6th generation)
            { "iPad14,5", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // iPad Pro (12.9-inch) (6th generation)
            { "iPad14,6", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "SILVER" }
                }
            },
            // iPad Air 6th Gen 11 inch
            { "iPad14,8", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "SILVER" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Air 6th Gen 11 inch
            { "iPad14,9", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "SILVER" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Air 11-inch (M3)
            { "iPad15,3", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Air 11-inch (M3)
            { "iPad15,4", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Air 13-inch (M3)
            { "iPad15,5", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Air 13-inch (M3)
            { "iPad15,6", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad 11th
            { "iPad15,7", new Dictionary<string, string>
                {
                    { "1", "YELLOW" },
                    { "2", "Silver" },
                    { "3", "Pink" },
                    { "4", "BLUE" }
                }
            },
            // iPad 11th
            { "iPad15,8", new Dictionary<string, string>
                {
                    { "1", "YELLOW" },
                    { "2", "Silver" },
                    { "3", "Pink" },
                    { "4", "BLUE" }
                }
            },
            // iPad mini 7
            { "iPad16,1", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad mini 7
            { "iPad16,2", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Pro 11-inch (M4)
            { "iPad16,3", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "Silver" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Pro 13-inch (M4)
            { "iPad16,4", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "Silver" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Pro 13-inch (M4)
            { "iPad16,5", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "Silver" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Pro 11-inch (M4)
            { "iPad16,6", new Dictionary<string, string>
                {
                    { "1", "Space Gray" },
                    { "2", "Silver" },
                    { "3", "Pink" },
                    { "4", "Blue" },
                    { "6", "Starlight" },
                    { "7", "Purple" }
                }
            },
            // iPad Air WiFi
            { "iPad4,1", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPad Air GSM+CDMA
            { "iPad4,2", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPad Air China
            { "iPad4,3", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPad mini Retina WiFi
            { "iPad4,4", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPad mini Retina GSM+CDMA
            { "iPad4,5", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPad mini Retina China
            { "iPad4,6", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPad mini 3 WiFi
            { "iPad4,7", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad mini 3 GSM+CDMA
            { "iPad4,8", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad Mini 3 China
            { "iPad4,9", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad mini 4 WiFi
            { "iPad5,1", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad mini 4 WiFi+Cellular
            { "iPad5,2", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad Air 2 WiFi
            { "iPad5,3", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad Air 2 Cellular
            { "iPad5,4", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad 2017 WiFi
            { "iPad6,11", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad 5th Gen Cellular
            { "iPad6,12", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad Pro 9.7 inch WiFi
            { "iPad6,3", new Dictionary<string, string>
                {
                    { "#b9b7ba", "SPACE GREY" },
                    { "#dadcdb", "SILVER" },
                    { "#e1ccb7", "GOLD" },
                    { "#e4c1b9", "ROSE GOLD" }
                }
            },
            // iPad Pro 9.7 inch Cellular
            { "iPad6,4", new Dictionary<string, string>
                {
                    { "#b9b7ba", "SPACE GREY" },
                    { "#dadcdb", "SILVER" },
                    { "#e1ccb7", "GOLD" },
                    { "#e4c1b9", "ROSE GOLD" }
                }
            },
            // iPad Pro 12.9 inch WiFi
            { "iPad6,7", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad Pro 12.9 inch Cellular
            { "iPad6,8", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPad Pro 2nd Gen (WiFi)
            { "iPad7,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad 7 WiFi
            { "iPad7,11", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad 7 Cellular
            { "iPad7,12", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 2nd Gen (WiFi+Cellular)
            { "iPad7,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPad Pro 10.5-inch WiFi
            { "iPad7,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 10.5-inch Cellular
            { "iPad7,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad 6th Gen (WiFi)
            { "iPad7,5", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad 6th Gen (WiFi+Cellular)
            { "iPad7,6", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 11 inch WiFi
            { "iPad8,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 11-inch 2nd Gen Cellular
            { "iPad8,10", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 4 12.9-inch (WiFi)
            { "iPad8,11", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 4 12.9-inch (Cellular)
            { "iPad8,12", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 11 inch 1TB, WiFi
            { "iPad8,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 11 inch Cellular
            { "iPad8,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 11 inch 1TB, Cellular
            { "iPad8,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 3rd Gen (12.9 inch, WiFi)
            { "iPad8,5", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 3rd Gen (12.9 inch, 1TB, WiFi)
            { "iPad8,6", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 3rd Gen (12.9 inch, WiFi+Cellular)
            { "iPad8,7", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 3rd Gen (12.9 inch, 1TB, WiFi+Cellular)
            { "iPad8,8", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPad Pro 11-inch 2nd Gen WiFi
            { "iPad8,9", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" }
                }
            },
            // iPhone 8
            { "iPhone10,1", new Dictionary<string, string>
                {
                    { "#e4e7e8", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" },
                    { "7", "GOLD" },
                    { "8", "SPACE GREY" }
                }
            },
            // iPhone 8 Plus
            { "iPhone10,2", new Dictionary<string, string>
                {
                    { "#e4e7e8", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" },
                    { "7", "GOLD" },
                    { "8", "SPACE GREY" }
                }
            },
            // iPhone X Global
            { "iPhone10,3", new Dictionary<string, string>
                {
                    { "#e4e7e8", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPhone 8
            { "iPhone10,4", new Dictionary<string, string>
                {
                    { "#e4e7e8", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" },
                    { "7", "GOLD" },
                    { "8", "SPACE GREY" }
                }
            },
            // iPhone 8 Plus
            { "iPhone10,5", new Dictionary<string, string>
                {
                    { "#e4e7e8", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" },
                    { "7", "GOLD" },
                    { "8", "SPACE GREY" }
                }
            },
            // iPhone X GSM
            { "iPhone10,6", new Dictionary<string, string>
                {
                    { "#e4e7e8", "GOLD" },
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPhone XS
            { "iPhone11,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "GOLD" }
                }
            },
            // iPhone XS Max China
            { "iPhone11,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "GOLD" }
                }
            },
            // iPhone XS Max
            { "iPhone11,6", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "GOLD" }
                }
            },
            // iPhone XR
            { "iPhone11,8", new Dictionary<string, string>
                {
                    { "1", "BLACK" },
                    { "2", "WHITE" },
                    { "6", "RED" },
                    { "7", "YELLOW" },
                    { "8", "CORAL" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 11
            { "iPhone12,1", new Dictionary<string, string>
                {
                    { "1", "BLACK" },
                    { "17", "PURPLE" },
                    { "18", "GREEN" },
                    { "2", "WHITE" },
                    { "6", "RED" },
                    { "7", "YELLOW" }
                }
            },
            // iPhone 11 Pro
            { "iPhone12,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "18", "MIDNIGHT GREEN" },
                    { "2", "SILVER" },
                    { "4", "GOLD" }
                }
            },
            // iPhone 11 Pro Max
            { "iPhone12,5", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "18", "MIDNIGHT GREEN" },
                    { "2", "SILVER" },
                    { "4", "GOLD" }
                }
            },
            // iPhone SE (2020)
            { "iPhone12,8", new Dictionary<string, string>
                {
                    { "1", "BLACK" },
                    { "2", "WHITE" },
                    { "6", "RED" }
                }
            },
            // iPhone 12 Mini
            { "iPhone13,1", new Dictionary<string, string>
                {
                    { "1", "BLACK" },
                    { "17", "PURPLE" },
                    { "18", "GREEN" },
                    { "2", "WHITE" },
                    { "6", "RED" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 12
            { "iPhone13,2", new Dictionary<string, string>
                {
                    { "1", "BLACK" },
                    { "17", "PURPLE" },
                    { "18", "GREEN" },
                    { "2", "WHITE" },
                    { "6", "RED" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 12 Pro
            { "iPhone13,3", new Dictionary<string, string>
                {
                    { "1", "GRAPHITE" },
                    { "18", "GOLD" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "9", "PACIFIC BLUE" }
                }
            },
            // iPhone 12 Pro Max
            { "iPhone13,4", new Dictionary<string, string>
                {
                    { "1", "GRAPHITE" },
                    { "18", "PACIFIC BLUE" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "9", "PACIFIC BLUE" }
                }
            },
            // iPhone 13 Pro
            { "iPhone14,2", new Dictionary<string, string>
                {
                    { "1", "GRAPHITE" },
                    { "18", "ALPINE GREEN" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "9", "SIERRA BLUE" }
                }
            },
            // iPhone 13 Pro Max
            { "iPhone14,3", new Dictionary<string, string>
                {
                    { "1", "GRAPHITE" },
                    { "18", "ALPINE GREEN" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "9", "SIERRA BLUE" }
                }
            },
            // iPhone 13 Mini
            { "iPhone14,4", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "18", "GREEN" },
                    { "2", "STARLIGHT" },
                    { "4", "PINK" },
                    { "6", "RED" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 13
            { "iPhone14,5", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "18", "GREEN" },
                    { "2", "STARLIGHT" },
                    { "4", "PINK" },
                    { "6", "RED" },
                    { "9", "BLUE" }
                }
            },
            // iPhone SE (3rd generation)
            { "iPhone14,6", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "2", "STARLIGHT" },
                    { "4", "PINK" },
                    { "6", "RED" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 14
            { "iPhone14,7", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "17", "PURPLE" },
                    { "2", "STARLIGHT" },
                    { "6", "RED" },
                    { "7", "YELLOW" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 14 Plus
            { "iPhone14,8", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "17", "PURPLE" },
                    { "2", "STARLIGHT" },
                    { "6", "RED" },
                    { "7", "YELLOW" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 14 Pro
            { "iPhone15,2", new Dictionary<string, string>
                {
                    { "1", "SPACE BLACK" },
                    { "17", "DEEP PURPLE" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPhone 14 Pro Max
            { "iPhone15,3", new Dictionary<string, string>
                {
                    { "1", "SPACE BLACK" },
                    { "17", "DEEP PURPLE" },
                    { "2", "SILVER" },
                    { "3", "GOLD" }
                }
            },
            // iPhone 15
            { "iPhone15,4", new Dictionary<string, string>
                {
                    { "1", "BLACK" },
                    { "18", "GREEN" },
                    { "4", "PINK" },
                    { "7", "YELLOW" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 15 Plus
            { "iPhone15,5", new Dictionary<string, string>
                {
                    { "1", "BLACK" },
                    { "18", "GREEN" },
                    { "4", "PINK" },
                    { "7", "YELLOW" },
                    { "9", "BLUE" }
                }
            },
            // iPhone 15 Pro
            { "iPhone16,1", new Dictionary<string, string>
                {
                    { "1", "BLACK TITANIUM" },
                    { "2", "WHITE TITANIUM" },
                    { "5", "NATURAL TITANIUM" },
                    { "9", "BLUE TITANIUM" }
                }
            },
            // iPhone 15 Pro Max
            { "iPhone16,2", new Dictionary<string, string>
                {
                    { "1", "BLACK TITANIUM" },
                    { "2", "WHITE TITANIUM" },
                    { "5", "NATURAL TITANIUM" },
                    { "9", "BLUE TITANIUM" }
                }
            },
            // iPhone 16 Pro
            { "iPhone17,1", new Dictionary<string, string>
                {
                    { "1", "Black Titanium" },
                    { "2", "White Titanium" },
                    { "4", "Desert Titanium" },
                    { "5", "Natural Titanium" }
                }
            },
            // iPhone 16 Pro Max
            { "iPhone17,2", new Dictionary<string, string>
                {
                    { "1", "Black Titanium" },
                    { "18", "Dark Cyan" },
                    { "2", "White Titanium" },
                    { "4", "Desert Titanium" },
                    { "5", "Natural Titanium" }
                }
            },
            // iPhone 16
            { "iPhone17,3", new Dictionary<string, string>
                {
                    { "1", "Black" },
                    { "18", "Teal" },
                    { "2", "White" },
                    { "4", "Pink" },
                    { "9", "Ultramarine" }
                }
            },
            // iPhone 16 Plus
            { "iPhone17,4", new Dictionary<string, string>
                {
                    { "1", "Black" },
                    { "18", "Teal" },
                    { "2", "White" },
                    { "4", "Pink" },
                    { "9", "Ultramarine" }
                }
            },
            // iPhone 16e
            { "iPhone17,5", new Dictionary<string, string>
                {
                    { "1", "Black" },
                    { "2", "White" }
                }
            },
            // iPhone 17 Pro
            { "iPhone18,1", new Dictionary<string, string>
                {
                    { "2", "Silver" },
                    { "8", "Cosmic Orange" },
                    { "9", "Deep Blue" }
                }
            },
            // iPhone 17 Pro Max
            { "iPhone18,2", new Dictionary<string, string>
                {
                    { "2", "Silver" },
                    { "8", "Cosmic Orange" },
                    { "9", "Deep Blue" }
                }
            },
            // iPhone 17
            { "iPhone18,3", new Dictionary<string, string>
                {
                    { "1", "Black" },
                    { "17", "Laverder" },
                    { "18", "Sage" },
                    { "2", "White" },
                    { "9", "Mist Blue" }
                }
            },
            // iPhone Air
            { "iPhone18,4", new Dictionary<string, string>
                {
                    { "1", "Space Black" },
                    { "2", "Cloud White" },
                    { "3", "Gold" },
                    { "9", "Sky Blue" }
                }
            },
            // iPhone 5 (GSM)
            { "iPhone5,1", new Dictionary<string, string>
                {
                    { "#99989b", "BLACK" },
                    { "#d7d9d8", "WHITE" }
                }
            },
            // iPhone 5 (GSM+CDMA)
            { "iPhone5,2", new Dictionary<string, string>
                {
                    { "#99989b", "BLACK" },
                    { "#d7d9d8", "WHITE" }
                }
            },
            // iPhone 5C (GSM)
            { "iPhone5,3", new Dictionary<string, string>
                {
                    { "#46abe0", "BLUE" },
                    { "#a1e877", "GREEN" },
                    { "#f5f4f7", "WHITE" },
                    { "#faf189", "YELLOW" },
                    { "#fe767a", "PINK" }
                }
            },
            // iPhone 5C (Global)
            { "iPhone5,4", new Dictionary<string, string>
                {
                    { "#46abe0", "BLUE" },
                    { "#a1e877", "GREEN" },
                    { "#f5f4f7", "WHITE" },
                    { "#faf189", "YELLOW" },
                    { "#fe767a", "PINK" }
                }
            },
            // iPhone 5S (GSM)
            { "iPhone6,1", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d4c5b3", "GOLD" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPhone 5S (Global)
            { "iPhone6,2", new Dictionary<string, string>
                {
                    { "#99989b", "SPACE GREY" },
                    { "#d4c5b3", "GOLD" },
                    { "#d7d9d8", "SILVER" }
                }
            },
            // iPhone 6 Plus
            { "iPhone7,1", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPhone 6
            { "iPhone7,2", new Dictionary<string, string>
                {
                    { "#b4b5b9", "SPACE GREY" },
                    { "#d7d9d8", "SILVER" },
                    { "#e1ccb5", "GOLD" }
                }
            },
            // iPhone 6s
            { "iPhone8,1", new Dictionary<string, string>
                {
                    { "#b9b7ba", "SPACE GREY" },
                    { "#dadcdb", "SILVER" },
                    { "#e1ccb7", "GOLD" },
                    { "#e4c1b9", "ROSE GOLD" }
                }
            },
            // iPhone 6s Plus
            { "iPhone8,2", new Dictionary<string, string>
                {
                    { "#b9b7ba", "SPACE GREY" },
                    { "#dadcdb", "SILVER" },
                    { "#e1ccb7", "GOLD" },
                    { "#e4c1b9", "ROSE GOLD" }
                }
            },
            // iPhone SE (GSM+CDMA)
            { "iPhone8,4", new Dictionary<string, string>
                {
                    { "#aeb1b8", "SPACE GREY" },
                    { "#d6c8b9", "GOLD" },
                    { "#dcdede", "SILVER" },
                    { "#e5bdb5", "ROSE GOLD" }
                }
            },
            // iPhone 7
            { "iPhone9,1", new Dictionary<string, string>
                {
                    { "#e1e4e3", "BLACK" },
                    { "1", "BLACK" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" }
                }
            },
            // iPhone 7 Plus
            { "iPhone9,2", new Dictionary<string, string>
                {
                    { "#e1e4e3", "BLACK" },
                    { "1", "BLACK" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" }
                }
            },
            // iPhone 7
            { "iPhone9,3", new Dictionary<string, string>
                {
                    { "#e1e4e3", "BLACK" },
                    { "1", "BLACK" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" }
                }
            },
            // iPhone 7 Plus
            { "iPhone9,4", new Dictionary<string, string>
                {
                    { "#e1e4e3", "BLACK" },
                    { "1", "BLACK" },
                    { "2", "SILVER" },
                    { "3", "GOLD" },
                    { "4", "ROSE GOLD" },
                    { "5", "JET BLACK" },
                    { "6", "RED" }
                }
            },
            // 5th Gen iPod
            { "iPod5,1", new Dictionary<string, string>
                {
                    { "#c6353f", "RED" },
                    { "sparrow", "SILVER" }
                }
            },
            // 6th Gen iPod
            { "iPod7,1", new Dictionary<string, string>
                {
                    { "#6b6a6d", "SPACE GREY" },
                    { "#c6353f", "RED" },
                    { "#e75090", "PINK" }
                }
            },
            // Apple Watch Series 2 (Nike+, 38mm)
            { "Watch 2,3", new Dictionary<string, string>
                {
                    { "1", "Silver" },
                    { "2", "Space Gray" }
                }
            },
            // Apple Watch Series 2 (Nike+, 42mm)
            { "Watch 2,4", new Dictionary<string, string>
                {
                    { "1", "Silver" },
                    { "2", "Space Gray" }
                }
            },
            // Apple Watch Series 3 Nike+ (38mm (GPS+Cellular)
            { "Watch 3,1", new Dictionary<string, string>
                {
                    { "1", "WHITE" },
                    { "2", "GREY" }
                }
            },
            // Apple Watch Series 3 Nike+ 42mm (GPS+Cellular)
            { "Watch 3,2", new Dictionary<string, string>
                {
                    { "1", "WHITE" },
                    { "2", "GREY" }
                }
            },
            // Apple Watch Series 3 Nike+ 38mm (GPS)
            { "Watch 3,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Apple Watch Series 3 Nike+ 42mm (GPS)
            { "Watch 3,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Apple Watch Series 4 Nike+ (GPS, 40mm)
            { "Watch 4,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "3", "SILVER" }
                }
            },
            // Apple Watch Series 4 Nike+ (GPS, 44mm)
            { "Watch 4,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "3", "SILVER" }
                }
            },
            // Apple Watch Series 4 Nike+ (GPS + Cellular, 40mm)
            { "Watch 4,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "3", "SILVER" }
                }
            },
            // Apple Watch Series 4 Nike+ (GPS + Cellular, 44mm)
            { "Watch 4,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "3", "SILVER" }
                }
            },
            // Apple Watch Series 4 HermÃ¨s (GPS + Cellular, 40mm)
            { "Watch 4,5", new Dictionary<string, string>
                {
                    { "1", "" }
                }
            },
            // Apple Watch Series 4 HermÃ¨s (GPS + Cellular, 44mm)
            { "Watch 4,6", new Dictionary<string, string>
                {
                    { "1", "" }
                }
            },
            // Watch Series 5 Nike (GPS, 40mm)
            { "Watch 5,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Watch Series 5 Nike (GPS, 44mm)
            { "Watch 5,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Watch Series 5 Nike (GPS + Cellular, 40mm)
            { "Watch 5,3", new Dictionary<string, string>
                {
                    { "1", "DARK TITANIUM" },
                    { "2", "LIGHT TITANIUM" },
                    { "3", "WHITE CERAMIC" }
                }
            },
            // Watch Series 5 Nike (GPS + Cellular, 44mm)
            { "Watch 5,4", new Dictionary<string, string>
                {
                    { "1", "DARK TITANIUM" },
                    { "2", "LIGHT TITANIUM" },
                    { "3", "WHITE CERAMIC" }
                }
            },
            // Apple Watch Series 6 Nike (GPS, 40mm)
            { "Watch 6,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Apple Watch Series 6 Nike (GPS, 44mm)
            { "Watch 6,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Apple Watch Series 6 Nike (GPS + Cellular, 40mm)
            { "Watch 6,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Apple Watch Series 6 Nike (GPS + Cellular, 44mm)
            { "Watch 6,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" }
                }
            },
            // Apple Watch Series 6 HermÃ¨s (GPS + Cellular, 40mm)
            { "Watch 6,5", new Dictionary<string, string>
                {
                    { "1", "TITANIUM" },
                    { "2", "SPACE BLACK TITANIUM" }
                }
            },
            // Apple Watch Series 6 HermÃ¨s (GPS + Cellular, 44mm)
            { "Watch 6,6", new Dictionary<string, string>
                {
                    { "1", "TITANIUM" },
                    { "2", "SPACE BLACK TITANIUM" }
                }
            },
            // Apple Watch Series 2 (Hermes, 38mm)
            { "Watch,2,3", new Dictionary<string, string>
                {
                    { "1", "" }
                }
            },
            // Apple Watch Series 2 (Hermes, 42mm)
            { "Watch,2,4", new Dictionary<string, string>
                {
                    { "1", "" }
                }
            },
            // Apple Watch Series 0 38mm
            { "Watch1,1", new Dictionary<string, string>
                {
                    { "2", "Space Black" },
                    { "3", "Stainless Steel" },
                    { "4", "ROSE GOLD" },
                    { "5", "SILVER" }
                }
            },
            // Apple Watch Series 0 42mm
            { "Watch1,2", new Dictionary<string, string>
                {
                    { "2", "Space Black" },
                    { "3", "Stainless Steel" },
                    { "4", "ROSE GOLD" },
                    { "5", "SILVER" }
                }
            },
            // Watch Series 2 38mm case
            { "Watch2,3", new Dictionary<string, string>
                {
                    { "1", "WHITE" },
                    { "2", "SPACE BLACK" },
                    { "3", "STAINLESS STEEL" },
                    { "4", "SPACE GREY" },
                    { "5", "GOLD" },
                    { "6", "ROSE GOLD" },
                    { "7", "SILVER" }
                }
            },
            // Watch Series 2 42mm case
            { "Watch2,4", new Dictionary<string, string>
                {
                    { "1", "WHITE" },
                    { "2", "SPACE BLACK" },
                    { "3", "STAINLESS STEEL" },
                    { "4", "SPACE GREY" },
                    { "5", "GOLD" },
                    { "6", "ROSE GOLD" },
                    { "7", "SILVER" }
                }
            },
            // Watch Series 1 38mm
            { "Watch2,6", new Dictionary<string, string>
                {
                    { "1", "WHITE" },
                    { "2", "SPACE BLACK" },
                    { "3", "STAINLESS STEEL" },
                    { "4", "SPACE GREY" },
                    { "5", "GOLD" },
                    { "6", "ROSE GOLD" },
                    { "7", "SILVER" }
                }
            },
            // Watch Series 1 42mm
            { "Watch2,7", new Dictionary<string, string>
                {
                    { "1", "WHITE" },
                    { "2", "SPACE BLACK" },
                    { "3", "STAINLESS STEEL" },
                    { "4", "SPACE GREY" },
                    { "5", "GOLD" },
                    { "6", "ROSE GOLD" },
                    { "7", "SILVER" }
                }
            },
            // Watch Series 3 38mm (GPS+Cellular)
            { "Watch3,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "SPACE BLACK" },
                    { "5", "STAINLESS STEEL" }
                }
            },
            // Watch Series 3 42mm (GPS+Cellular)
            { "Watch3,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "SPACE BLACK" },
                    { "5", "STAINLESS STEEL" }
                }
            },
            // Watch Series 3 38mm (GPS)
            { "Watch3,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "SPACE BLACK" },
                    { "5", "STAINLESS STEEL" }
                }
            },
            // Watch Series 3 42mm (GPS)
            { "Watch3,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "SPACE BLACK" },
                    { "5", "STAINLESS STEEL" }
                }
            },
            // Watch Series 4 40mm case (GPS)
            { "Watch4,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch Series 4 44mm case (GPS)
            { "Watch4,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch Series 4 40mm case (GPS+Cellular)
            { "Watch4,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch Series 4 44mm (GPS+Cellular)
            { "Watch4,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch Series 5 GPS (40mm)
            { "Watch5,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch SE GPS (44mm)
            { "watch5,10", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "SILVER" }
                }
            },
            // Watch SE (40mm) (GPS+Cellular)
            { "watch5,11", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "SILVER" }
                }
            },
            // Watch SE (44mm) (GPS+Cellular)
            { "watch5,12", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "SILVER" }
                }
            },
            // Watch Series 5 GPS (44mm)
            { "Watch5,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch Series 5 (40mm, LTE)
            { "Watch5,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch Series 5 (44mm, LTE)
            { "Watch5,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "STAINLESS STEEL" },
                    { "5", "SPACE BLACK" }
                }
            },
            // Watch SE GPS (40mm)
            { "watch5,9", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "2", "SILVER" },
                    { "3", "SILVER" }
                }
            },
            // Apple Watch Series 6 40mm Wi-Fi
            { "Watch6,1", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "10", "SPACE BLACK STAINLESS STEEL" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "BLUE" },
                    { "6", "SILVER STAINLESS STEEL" },
                    { "7", "GRAPHITE STAINLESS STEEL" },
                    { "8", "GOLD STAINLESS STEEL" },
                    { "9", "TITANIUM" }
                }
            },
            // Apple Watch SE (2nd generation) 40mm Wi-Fi 
            { "Watch6,10", new Dictionary<string, string>
                {
                    { "1", "Starlight" },
                    { "2", "Midnight" },
                    { "32", "Silver" }
                }
            },
            // Apple Watch SE (2nd generation) 44mm Wi-Fi 
            { "Watch6,11", new Dictionary<string, string>
                {
                    { "1", "Starlight" },
                    { "2", "Midnight" },
                    { "32", "Silver" }
                }
            },
            // Apple Watch SE (2nd generation) 40mm Cellular 
            { "Watch6,12", new Dictionary<string, string>
                {
                    { "1", "Starlight" },
                    { "2", "Midnight" },
                    { "32", "Silver" }
                }
            },
            // Apple Watch SE (2nd generation) 44mm Cellular 
            { "Watch6,13", new Dictionary<string, string>
                {
                    { "1", "Starlight" },
                    { "2", "Midnight" },
                    { "32", "Silver" }
                }
            },
            //  Apple Watch Series 8 41mm Wi-Fi
            { "Watch6,14", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "2", "STARLIGHT" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "GRAPHITE" },
                    { "6", "GOLD" },
                    { "7", "SPACE BLACK" }
                }
            },
            // Apple Watch Series 8 45mm Wi-Fi 
            { "Watch6,15", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "2", "STARLIGHT" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "GRAPHITE" },
                    { "6", "GOLD" },
                    { "7", "SPACE BLACK" }
                }
            },
            // Apple Watch Series 8 41mm Cellular 
            { "Watch6,16", new Dictionary<string, string>
                {
                    { "1", "Starlight" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "Midnight" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "Silver" },
                    { "6", "Graphite" },
                    { "7", "SPACE BLACK" }
                }
            },
            // Apple Watch Series 8 45mm Cellular 
            { "Watch6,17", new Dictionary<string, string>
                {
                    { "1", "MIDNIGHT" },
                    { "2", "STARLIGHT" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "GRAPHITE" },
                    { "6", "GOLD" },
                    { "7", "SPACE BLACK" }
                }
            },
            // Apple Watch Ultra
            { "Watch6,18", new Dictionary<string, string>
                {
                    { "8", "" }
                }
            },
            // Apple Watch Series 6 44mm Wi-Fi 
            { "Watch6,2", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "10", "SPACE BLACK STAINLESS STEEL" },
                    { "2", "GOLD" },
                    { "26", "BLUE" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "BLUE" },
                    { "6", "SILVER STAINLESS STEEL" },
                    { "7", "GRAPHITE STAINLESS STEEL" },
                    { "8", "GOLD STAINLESS STEEL" },
                    { "9", "TITANIUM" }
                }
            },
            // Apple Watch Series 6 40mm LTE 
            { "Watch6,3", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "10", "SPACE BLACK STAINLESS STEEL" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "BLUE" },
                    { "6", "SILVER STAINLESS STEEL" },
                    { "7", "GRAPHITE STAINLESS STEEL" },
                    { "8", "GOLD STAINLESS STEEL" },
                    { "9", "TITANIUM" }
                }
            },
            // Apple Watch Series 6 44mm LTE 
            { "Watch6,4", new Dictionary<string, string>
                {
                    { "1", "SPACE GREY" },
                    { "10", "SPACE BLACK STAINLESS STEEL" },
                    { "2", "GOLD" },
                    { "3", "SILVER" },
                    { "4", "RED" },
                    { "5", "BLUE" },
                    { "6", "SILVER STAINLESS STEEL" },
                    { "7", "GRAPHITE STAINLESS STEEL" },
                    { "8", "GOLD STAINLESS STEEL" },
                    { "9", "TITANIUM" }
                }
            },
            // Apple Watch Series 7 41mm Wi-Fi 
            { "Watch6,6", new Dictionary<string, string>
                {
                    { "1", "STARTLIGHT" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "MIDNIGHT" },
                    { "22", "Titanium" },
                    { "26", "Blue" },
                    { "3", "GREEN" },
                    { "5", "Silver" },
                    { "6", "Graphite" },
                    { "7", "Green" },
                    { "8", "Silver" }
                }
            },
            // Apple Watch Series 7 45mm Wi-Fi 
            { "Watch6,7", new Dictionary<string, string>
                {
                    { "1", "STARTLIGHT" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "MIDNIGHT" },
                    { "22", "Titanium" },
                    { "26", "Blue" },
                    { "3", "GREEN" },
                    { "5", "Silver" },
                    { "6", "Graphite" },
                    { "7", "Green" },
                    { "8", "Silver" }
                }
            },
            // Apple Watch Series 7 41mm LTE 
            { "Watch6,8", new Dictionary<string, string>
                {
                    { "1", "STARTLIGHT" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "MIDNIGHT" },
                    { "22", "Titanium" },
                    { "26", "Blue" },
                    { "3", "GREEN" },
                    { "5", "Silver" },
                    { "6", "Graphite" },
                    { "7", "Green" },
                    { "8", "Silver" }
                }
            },
            // Apple Watch Series 7 45mm LTE 
            { "Watch6,9", new Dictionary<string, string>
                {
                    { "1", "STARTLIGHT" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "MIDNIGHT" },
                    { "22", "Titanium" },
                    { "26", "Blue" },
                    { "3", "GREEN" },
                    { "5", "Silver" },
                    { "6", "Graphite" },
                    { "7", "Green" },
                    { "8", "Silver" }
                }
            },
            // Apple Watch Series 7 Nike (GPS, 41mm)
            { "Watch7,1", new Dictionary<string, string>
                {
                    { "1", "Silver" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "Midnight" },
                    { "31", "Pink" },
                    { "32", "Starlight" },
                    { "5", "Silver" },
                    { "6", "Graphite" }
                }
            },
            // Apple Watch Series 10
            { "Watch7,10", new Dictionary<string, string>
                {
                    { "23", "Gold" },
                    { "32", "Silver" },
                    { "34", "Jet Black" },
                    { "36", "Natural Titanium" },
                    { "38", "Slate Titanium" },
                    { "4", "Rose Gold" }
                }
            },
            // Apple Watch Series 10
            { "Watch7,11", new Dictionary<string, string>
                {
                    { "23", "Gold" },
                    { "32", "Silver" },
                    { "34", "Jet Black" },
                    { "36", "Natural Titanium" },
                    { "38", "Slate Titanium" },
                    { "4", "Rose Gold" }
                }
            },
            // Apple Watch Series 7 Nike (GPS, 45mm)
            { "Watch7,2", new Dictionary<string, string>
                {
                    { "1", "Silver" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "Midnight" },
                    { "31", "Pink" },
                    { "32", "Starlight" },
                    { "5", "Silver" },
                    { "6", "Graphite" }
                }
            },
            // Apple Watch Series 9
            { "Watch7,3", new Dictionary<string, string>
                {
                    { "1", "Silver" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "Midnight" },
                    { "31", "Pink" },
                    { "32", "Starlight" },
                    { "5", "Silver" },
                    { "6", "Graphite" }
                }
            },
            // Apple Watch Series 7 Nike (GPS + Cellular, 45mm)
            { "Watch7,4", new Dictionary<string, string>
                {
                    { "1", "Silver" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "Midnight" },
                    { "31", "Pink" },
                    { "32", "Starlight" },
                    { "5", "Silver" },
                    { "6", "Graphite" }
                }
            },
            // Apple Watch Ultra 2
            { "Watch7,5", new Dictionary<string, string>
                {
                    { "14", "Black" },
                    { "8", "Titanium" }
                }
            },
            // Apple Watch Series 7 HermÃ¨s (GPS + Cellular, 45mm)
            { "Watch7,6", new Dictionary<string, string>
                {
                    { "1", "SILVER" },
                    { "2", "SPACE BLACK" }
                }
            },
            // Apple Watch Series 10
            { "Watch7,8", new Dictionary<string, string>
                {
                    { "23", "Gold" },
                    { "32", "Silver" },
                    { "34", "Jet Black" },
                    { "36", "Natural Titanium" },
                    { "38", "Slate Titanium" },
                    { "4", "Rose Gold" }
                }
            },
            // Apple Watch Series 10
            { "Watch7,9", new Dictionary<string, string>
                {
                    { "23", "Gold" },
                    { "32", "Silver" },
                    { "34", "Jet Black" },
                    { "36", "Natural Titanium" },
                    { "38", "Slate Titanium" },
                    { "4", "Rose Gold" }
                }
            },
            // Apple Watch Series 8 HermÃ¨s (GPS + Cellular, 41mm)
            { "Watch8,1", new Dictionary<string, string>
                {
                    { "1", "Starlight" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "Midnight" },
                    { "5", "Silver" },
                    { "6", "Graphite" }
                }
            },
            // Apple Watch Series 8 HermÃ¨s (GPS + Cellular, 45mm)
            { "Watch8,2", new Dictionary<string, string>
                {
                    { "1", "Starlight" },
                    { "12", "Gold" },
                    { "18", "Red" },
                    { "2", "Midnight" },
                    { "5", "Silver" },
                    { "6", "Graphite" }
                }
            }
        };

        private static readonly Dictionary<string, string> NameToHexMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "SPACE GREY", "#8E8E93" },
            { "SPACE GRAY", "#8E8E93" },
            { "SILVER", "#E3E4E5" },
            { "GOLD", "#F9E5C9" },
            { "ROSE GOLD", "#EABFB9" },
            { "SPACE BLACK", "#353535" },
            { "MIDNIGHT", "#1D2327" },
            { "STARLIGHT", "#FAF7F2" },
            { "BLUE", "#215E7C" },
            { "RED", "#A00008" },
            { "GREEN", "#315044" },
            { "YELLOW", "#F9E46B" },
            { "PINK", "#EBCAD6" },
            { "PURPLE", "#D1D1EB" },
            { "GRAPHITE", "#4B4946" },
            { "JET BLACK", "#0A0A0A" },
            { "BLACK", "#1F2020" },
            { "WHITE", "#FFFFFF" },
            { "SKY BLUE", "#DEF1FD" },
            { "ALPINE GREEN", "#505A4E" },
            { "SIERRA BLUE", "#9BB5CE" },
            { "PACIFIC BLUE", "#2E4755" },
            { "STAINLESS STEEL", "#D7D7D7" },
            { "TITANIUM", "#A2A4A5" },
            { "NATURAL TITANIUM", "#BEBCB9" },
            { "BLUE TITANIUM", "#4B525A" },
            { "WHITE TITANIUM", "#F2F1ED" },
            { "BLACK TITANIUM", "#424343" },
            { "DESERT TITANIUM", "#D3C2A6" },
            { "CORAL", "#FF6F61" },
            { "ORANGE", "#FF9500" },
            { "YELLOW GOLD", "#EFD38E" }
        };

        public static string GetColorName(string? productType, string? enclosureCode)
        {
            if (string.IsNullOrWhiteSpace(productType) || string.IsNullOrWhiteSpace(enclosureCode)) return enclosureCode ?? string.Empty;
            if (ColorMap.TryGetValue(productType, out var colorMappings))
            {
                if (colorMappings.TryGetValue(enclosureCode, out var colorName)) return colorName;
            }
            return enclosureCode;
        }

        public static string GetColorHex(string? productType, string? enclosureCode)
        {
            if (string.IsNullOrWhiteSpace(enclosureCode)) return "Transparent";

            // If it's already a hex starting with #, return it as is
            if (enclosureCode.StartsWith("#")) return enclosureCode;

            // Try to find if the enclosureCode itself is a key that maps to a name
            if (ColorMap.TryGetValue(productType ?? "", out var mappings))
            {
                if (mappings.TryGetValue(enclosureCode, out var name))
                {
                    if (name.StartsWith("#")) return name;
                    
                    // Exact match
                    if (NameToHexMap.TryGetValue(name, out var hex)) return hex;

                    // Partial match (e.g. "Space Gray (Aluminum)" contains "Space Gray")
                    foreach (var kv in NameToHexMap)
                    {
                        if (name.IndexOf(kv.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                            return kv.Value;
                    }
                }
            }

            // Fallback: try direct name match if enclosureCode is actually a color name
            if (NameToHexMap.TryGetValue(enclosureCode, out var directHex)) return directHex;
            
            // Partial match fallback
            foreach (var kv in NameToHexMap)
            {
                if (enclosureCode.IndexOf(kv.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return kv.Value;
            }

            // Last resort: if it's 6 hex chars, add #
            if (enclosureCode.Length == 6 && System.Linq.Enumerable.All(enclosureCode, c => "0123456789ABCDEFabcdef".Contains(c)))
                return "#" + enclosureCode;

            return "Transparent";
        }

    }
}


