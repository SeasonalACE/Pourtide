DELETE FROM `weenie` WHERE `class_Id` = 602001;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (602001, 'ace602001-salvagebarrel', 21, '2024-04-26 23:49:53') /* Container */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (602001,   1,        512) /* ItemType - Container */
     , (602001,   5,         50) /* EncumbranceVal */
     , (602001,   6,        102) /* ItemsCapacity */
     , (602001,   8,         50) /* Mass */
     , (602001,   9,          0) /* ValidLocations - None */
     , (602001,  16,         56) /* ItemUseable - ContainedViewedRemote */
     , (602001,  19,        100) /* Value */
     , (602001,  33,          1) /* Bonded - Bonded */
     , (602001,  74, 1073741824) /* MerchandiseItemTypes - TinkeringMaterial */
     , (602001,  93,       1044) /* PhysicsState - Ethereal, IgnoreCollisions, Gravity */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (602001,  13, True ) /* Ethereal */
     , (602001,  22, True ) /* Inscribable */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (602001,   1, 'Salvage Barrel') /* Name */
     , (602001,  15, 'Can only hold salvage.') /* ShortDesc */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (602001,   1,   33554597) /* Setup */
     , (602001,   3,  536870932) /* SoundTable */
     , (602001,   8,  100675552) /* Icon */
     , (602001,  22,  872415275) /* PhysicsEffectTable */;

/* Lifestoned Changelog:
{
  "LastModified": "2024-04-26T16:40:42.5331712-07:00",
  "ModifiedBy": "pourman",
  "Changelog": [],
  "UserChangeSummary": "add salvage barrel",
  "IsDone": false
}
*/
