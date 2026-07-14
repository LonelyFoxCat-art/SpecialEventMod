using UnityEngine;

namespace SpecialEvent;

public class PassiveAbility_test : PassiveAbilityBase {
    public override void OnWaveStart() {
        Debug.Log("PassiveAbility_test");
    }
}