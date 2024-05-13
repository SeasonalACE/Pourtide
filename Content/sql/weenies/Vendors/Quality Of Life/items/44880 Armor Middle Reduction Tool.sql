DELETE FROM `weenie` WHERE `class_Id` = 44880;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (44880, 'ace44880-armormiddlereductiontool', 38, '2024-05-13 21:40:25') /* Gem */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (44880,   1,       2048) /* ItemType - Gem */
     , (44880,   5,         10) /* EncumbranceVal */
     , (44880,  11,          1) /* MaxStackSize */
     , (44880,  12,          1) /* StackSize */
     , (44880,  16,     524296) /* ItemUseable - SourceContainedTargetContained */
     , (44880,  19,          1) /* Value */
     , (44880,  53,        101) /* PlacementPosition - Resting */
     , (44880,  93,       1044) /* PhysicsState - Ethereal, IgnoreCollisions, Gravity */
     , (44880,  94,          6) /* TargetType - Vestements */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (44880,  11, True ) /* IgnoreCollisions */
     , (44880,  13, True ) /* Ethereal */
     , (44880,  14, True ) /* GravityStatus */
     , (44880,  19, True ) /* Attackable */
     , (44880,  22, True ) /* Inscribable */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (44880,   1, 'Armor Middle Reduction Tool') /* Name */
     , (44880,  14, 'Use this tool on any loot generated multi-slot armor in order to reduce it to a single slot. It will still cover the same slots in appearance but only a single slot in armor coverage. ') /* Use */
     , (44880,  16, 'This tool will reduce Leggings to Tasset Coverage.') /* LongDesc */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (44880,   1,   33555677) /* Setup */
     , (44880,   3,  536870932) /* SoundTable */
     , (44880,   8,  100692210) /* Icon */
     , (44880,  22,  872415275) /* PhysicsEffectTable */;

/* Lifestoned Changelog:
{
  "LastModified": "2024-05-13T14:36:10.275361-07:00",
  "ModifiedBy": "pourman",
  "Changelog": [
    {
      "created": "2019-04-16T22:02:37",
      "author": "Chosenone",
      "comment": "Added Use/Long Description "
    },
    {
      "created": "2019-05-04T10:39:21.620062-07:00",
      "author": "TectonicRifts",
      "comment": "Corrected long description."
    }
  ],
  "UserChangeSummary": "Corrected long description.",
  "IsDone": true
}
*/
