DELETE FROM `weenie` WHERE `class_Id` = 42622;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (42622, 'ace42622-armormainreductiontool', 38, '2024-05-13 21:40:25') /* Gem */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (42622,   1,       2048) /* ItemType - Gem */
     , (42622,   5,         10) /* EncumbranceVal */
     , (42622,  11,          1) /* MaxStackSize */
     , (42622,  12,          1) /* StackSize */
     , (42622,  16,     524296) /* ItemUseable - SourceContainedTargetContained */
     , (42622,  19,          1) /* Value */
     , (42622,  53,        101) /* PlacementPosition - Resting */
     , (42622,  93,       1044) /* PhysicsState - Ethereal, IgnoreCollisions, Gravity */
     , (42622,  94,          6) /* TargetType - Vestements */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (42622,  11, True ) /* IgnoreCollisions */
     , (42622,  13, True ) /* Ethereal */
     , (42622,  14, True ) /* GravityStatus */
     , (42622,  19, True ) /* Attackable */
     , (42622,  22, True ) /* Inscribable */
     , (42622,  69, False) /* IsSellable */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (42622,   1, 'Armor Main Reduction Tool') /* Name */
     , (42622,  14, 'Use this tool on any loot generated multi-slot armor in order to reduce it to a single slot. It will still cover the same slots in appearance but only a single slot in armor coverage.') /* Use */
     , (42622,  16, 'This tool will reduce Hauberk/Coats/Cuirass to Breastplate coverage. It will reduce Sleeves to Pauldron coverage and Leggings to Girth coverage.') /* LongDesc */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (42622,   1,   33555677) /* Setup */
     , (42622,   3,  536870932) /* SoundTable */
     , (42622,   8,  100692208) /* Icon */
     , (42622,  22,  872415275) /* PhysicsEffectTable */;

/* Lifestoned Changelog:
{
  "LastModified": "2024-05-13T14:33:34.1842412-07:00",
  "ModifiedBy": "pourman",
  "Changelog": [
    {
      "created": "2019-04-16T19:00:40.9250648-07:00",
      "author": "Chosenone",
      "comment": "Checked and verified"
    }
  ],
  "UserChangeSummary": "Checked and verified",
  "IsDone": true
}
*/
