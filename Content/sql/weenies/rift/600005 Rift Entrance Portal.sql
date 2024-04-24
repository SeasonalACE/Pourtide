DELETE FROM `weenie` WHERE `class_Id` = 600005;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (600005, 'ace600005-riftentranceportal', 7, '2024-04-24 11:40:56') /* Portal */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (600005,   1,      65536) /* ItemType - Portal */
     , (600005,   3,         26) /* PaletteTemplate - DarkGoldMetal */
     , (600005,  16,         32) /* ItemUseable - Remote */
     , (600005,  93,       2052) /* PhysicsState - Ethereal, LightingOn */
     , (600005, 111,          1) /* PortalBitmask - Unrestricted */
     , (600005, 133,          4) /* ShowableOnRadar - ShowAlways */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (600005,   1, True ) /* Stuck */
     , (600005,  11, False) /* IgnoreCollisions */
     , (600005,  12, False) /* ReportCollisions */
     , (600005,  13, True ) /* Ethereal */
     , (600005,  14, False) /* GravityStatus */
     , (600005,  15, True ) /* LightsStatus */
     , (600005,  88, False) /* PortalShowDestination */;

INSERT INTO `weenie_properties_float` (`object_Id`, `type`, `value`)
VALUES (600005,  12,     0.5) /* Shade */
     , (600005,  54,    0.75) /* UseRadius */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (600005,   1, 'Rift Entrance Portal') /* Name */
     , (600005,  14, 'A portal that sends you to a random rift dungeon!') /* Use */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (600005,   1,   33556212) /* Setup */
     , (600005,   2,  150994947) /* MotionTable */
     , (600005,   6,   67109370) /* PaletteBase */
     , (600005,   7,  268435652) /* ClothingBase */
     , (600005,   8,  100667499) /* Icon */;

/* Lifestoned Changelog:
{
  "LastModified": "2024-04-24T04:40:17.2582633-07:00",
  "ModifiedBy": "pourman",
  "Changelog": [],
  "UserChangeSummary": "Cache + Show Portal Dest",
  "IsDone": true
}
*/
