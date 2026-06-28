# Asset Registry

Generated for Stage 0 from the provided concept-card labels and copied source images. Labels are working references; game code uses safe working IDs.

| Original label | Safe working ID | Category | Factory | Cost | Ticks | Power | IP review | Source file |
| --- | --- | --- | --- | ---: | ---: | --- | --- | --- |
| Light Tank | `light_tank` | vehicle | war_factory | 500 | 30 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (1).png |
| Medium Tank | `medium_tank` | vehicle | war_factory | 700 | 38 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (2).png |
| Heavy Tank | `heavy_tank` | vehicle | war_factory | 950 | 48 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (3).png |
| Gunner | `rifle_infantry` | infantry | barracks | 100 | 10 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (4).png |
| Grenadier | `grenade_infantry` | infantry | barracks | 130 | 12 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (5).png |
| Rocket Soldier | `rocket_infantry` | infantry | barracks | 180 | 16 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (6).png |
| Flame Trooper | `flame_infantry` | infantry | barracks | 170 | 16 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (7).png |
| Engineer | `engineer` | infantry | barracks | 250 | 20 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_29 PM (8).png |
| Fabrication Hub | `fabrication_hub` | building | starting_unit | 0 | 0 | +25 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (1).png |
| MASH | `field_hospital` | support | fabrication_hub | 450 | 28 | -6 | True | ChatGPT Image Jun 28, 2026, 02_14_42 PM (10).png |
| Barracks | `barracks` | building | fabrication_hub | 350 | 24 | -8 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (2).png |
| War Factory | `war_factory` | building | fabrication_hub | 700 | 35 | -15 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (3).png |
| Refinery | `refinery` | building | fabrication_hub | 600 | 32 | -12 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (4).png |
| Harvester | `harvester` | vehicle | war_factory | 700 | 35 | 0 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (5).png |
| Power Plant | `power_plant` | building | fabrication_hub | 300 | 20 | +40 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (6).png |
| Large Power Plant | `advanced_power_plant` | building | fabrication_hub | 650 | 32 | +80 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (7).png |
| Comm Center | `comm_center` | support | fabrication_hub | 800 | 45 | -14 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (8).png |
| Repair Bay | `repair_bay` | support | fabrication_hub | 500 | 30 | -10 | False | ChatGPT Image Jun 28, 2026, 02_14_42 PM (9).png |
| Tech Center | `tech_center` | support | fabrication_hub | 1200 | 60 | -18 | False | ChatGPT Image Jun 28, 2026, 02_14_54 PM (1).png |
| Turret | `cannon_turret` | defense | fabrication_hub | 400 | 26 | -8 | False | ChatGPT Image Jun 28, 2026, 02_14_54 PM (2).png |
| Gun Tower | `gun_tower` | defense | fabrication_hub | 250 | 18 | -6 | False | ChatGPT Image Jun 28, 2026, 02_14_54 PM (3).png |
| Advanced Gun Tower | `advanced_gun_tower` | defense | fabrication_hub | 550 | 34 | -12 | False | ChatGPT Image Jun 28, 2026, 02_14_54 PM (4).png |
| Scout Rover | `scout_rover` | vehicle | war_factory | 300 | 20 | 0 | False | ChatGPT Image Jun 28, 2026, 02_15_02 PM (1).png |
| APC | `apc` | vehicle | war_factory | 600 | 34 | 0 | False | ChatGPT Image Jun 28, 2026, 02_15_02 PM (2).png |
| Skyraider | `attack_aircraft` | aircraft | dual_helipad | 800 | 45 | 0 | True | ChatGPT Image Jun 28, 2026, 02_15_11 PM (1).png |
| Orca Lifter | `heavy_lifter_aircraft` | aircraft | dual_helipad | 900 | 50 | 0 | True | ChatGPT Image Jun 28, 2026, 02_15_11 PM (2).png |
| Dual Helipad | `dual_helipad` | support | fabrication_hub | 700 | 36 | -12 | False | ChatGPT Image Jun 28, 2026, 02_15_11 PM (3).png |

## Notes

- `assets.json` is the machine-readable registry for tools and future Unity import work.
- `Orca Lifter`, `Skyraider`, and `MASH` are treated as working labels that need release-name review.
- Concept cards are references only. Final production assets should be original high-detail models, textures, animation rigs, and effects.
