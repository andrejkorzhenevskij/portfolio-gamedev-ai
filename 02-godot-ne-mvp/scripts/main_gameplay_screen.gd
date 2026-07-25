extends Control

const TITLE_SCENE_PATH := "res://scenes/start/TitleScreen.tscn"
const GAMEPLAY_SCENE_PATH := "res://scenes/gameplay/GameplayScreen.tscn"
const SURGERY_SCENE_PATH := "res://scenes/gameplay/SurgeryLayer.tscn"
const SNAPSHOT_SCENE_PATH := "res://scenes/gameplay/SnapshotScreen.tscn"
const FINAL_SCENE_PATH := "res://scenes/gameplay/FinalScreen.tscn"
const F1_SCRIPT_PATH := "res://narrative/F1_script.txt"
const F2_SCRIPT_PATH := "res://narrative/F2_script.txt"
const F3_SCRIPT_PATH := "res://narrative/F3_script.txt"
const PHASE_FLOW := ["F1", "F2", "F3"]
const FILM_METRIC_MAX := 9.0
const STEP_SEPARATOR := "::step"
const AUTHORED_LINE_COMMAND_PREFIX := "::"
const OCTAVIA_AUTO_CLEAR_CLICKS := 2
const TURN_CLICK_COUNT := 3
const FADE_DURATION := 1.5
const EVENT_FX_FADE_IN_DURATION := 0.35
const EVENT_FX_FADE_OUT_DURATION := 0.24
const PAN_DEFAULT_DURATION := 2.5
const BODY_COPY_EMPHASIS_DURATION := 0.24
const BODY_COPY_EMPHASIS_OFFSET_Y := -8.0
const BODY_COPY_EMPHASIS_START_ALPHA := 0.58
const PAN_JITTER_AMPLITUDE := 34.0
const TURN_GLITCH_SHIFT := 18.0
const FRAME_STRIP_BASE_WIDTH := 40.0
const FRAME_STRIP_MIN_WIDTH := 24.0
const FRAME_VIEWPORT_MIN_WIDTH := 180.0
const FRAME_SIDE_GUTTER := 12.0
const FRAME_TOP_MATTE := 18.0
const FRAME_BOTTOM_MATTE := 18.0
const FRAME_STRIP_TO_PORTRAIT_GAP := 22.0
const FILMSTRIP_LOOP_SPEED := 27.0
const FILMSTRIP_LOOP_OVERFLOW := 48.0
const PORTRAIT_STRIP_BASE_HEIGHT := 92.0
const PORTRAIT_STRIP_MIN_HEIGHT := 76.0
const PORTRAIT_STRIP_SIDE_INSET := 14.0
const CONTINUE_PROMPT_COPY := "Клик — продолжить"
const EVENT_FX_NONE := ""
const EVENT_FX_OBLIVION := "oblivion"
const EVENT_FX_BURN := "burn"
const HUD_CAST_CHARACTER_ORDER := ["desmond", "victoria", "leonard"]
const HUD_CAST_CHARACTER_SHORT_NAMES := {
	"desmond": "Д",
	"victoria": "В",
	"leonard": "Л",
}
const HUD_CAST_LEGEND_COPY := "СБ — самобытность. СЛ — след."

const PAN_PRESETS := [
	{
		"name": "drift_right",
		"start_scale": Vector2(1.02, 1.02),
		"end_scale": Vector2(1.08, 1.08),
		"start_offset": Vector2(-12.0, -6.0),
		"end_offset": Vector2(20.0, -14.0),
	},
	{
		"name": "drift_down",
		"start_scale": Vector2(1.03, 1.03),
		"end_scale": Vector2(1.09, 1.09),
		"start_offset": Vector2(10.0, -18.0),
		"end_offset": Vector2(-8.0, 16.0),
	},
	{
		"name": "slow_push",
		"start_scale": Vector2(1.00, 1.00),
		"end_scale": Vector2(1.07, 1.07),
		"start_offset": Vector2(0.0, 0.0),
		"end_offset": Vector2(10.0, -10.0),
	},
]

const IMAGE_LIBRARY := {
	"image_1": "res://art/image_1.png",
	"image_2": "res://art/image_2.png",
	"image_3": "res://art/image_3.png",
	"image_4": "res://art/image_4.png",
	"image_4-2.png": "res://art/image_4-2.png",
	"image_5": "res://art/image_5.png",
	"image_6": "res://art/image_6.png",
	"image_7": "res://art/image_7.png",
	"image_8": "res://art/image_8.png",
	"image_9": "res://art/image_9.png",
	"image_10": "res://art/image_10.png",
	"image_11": "res://art/image_11.png",
	"image_12": "res://art/image_12.png",
	"image_13": "res://art/image_13.png",
	"image_14": "res://art/image_14.png",
	"image_15": "res://art/image_15.png",
	"image_16": "res://art/image_16.png",
	"image_18": "res://art/image_18.png",
	"image_19": "res://art/image_19.png",
	"image_20": "res://art/image_20.png",
	"image_21": "res://art/image_21.png",
	"image_22": "res://art/image_22.png",
	"image_23": "res://art/image_23.png",
	"image_24": "res://art/image_24.png",
	"image_25": "res://art/image_25.png",
	"image_26": "res://art/image_26.png",
	"image_27": "res://art/image_27.png",
	"image_28": "res://art/image_28.png",
	"image_29": "res://art/image_29.png",
	"image_30": "res://art/image_30.png",
	"image_31": "res://art/image_31.png",
	"image_32": "res://art/image_32.png",
	"image_33": "res://art/image_33.png",
	"image_34": "res://art/image_34.png",
	"image_35": "res://art/image_35.png",
	"image_36": "res://art/image_36.png",
	"image_37": "res://art/image_37.png",
	"image_38": "res://art/image_38.png",
	"image_39": "res://art/image_39.png",
	"image_40": "res://art/image_40.png",
	"image_41": "res://art/image_41.png",
	"image_42": "res://art/image_42.png",
	"image_43": "res://art/image_43.png",
	"image_44": "res://art/image_44.png",
	"image_45": "res://art/image_45.png",
	"image_46": "res://art/image_46.png",
	"image_46-1": "res://art/image_46-1.png",
	"image_46-1.png": "res://art/image_46-1.png",
	"image_47": "res://art/image_47.png",
	"image_47-1": "res://art/image_47-1.png",
	"image_47-1.png": "res://art/image_47-1.png",
	"image_48": "res://art/image_48.png",
	"image_48-1": "res://art/image_48-1.png",
	"image_48-1.png": "res://art/image_48-1.png",
	"image_48-2": "res://art/image_48-2.png",
	"image_48-2.png": "res://art/image_48-2.png",
	"image_49": "res://art/image_49.png",
	"image_49-1": "res://art/image_49-1.png",
	"image_49-1.png": "res://art/image_49-1.png",
	"image_50": "res://art/image_50.png",
	"image_51": "res://art/image_51.png",
	"image_52": "res://art/image_52.png",
	"prep_bay": "res://art/image_1.png",
	"corridor_flash": "res://art/image_2.png",
	"theater_hold": "res://art/image_5.png",
	"resolution_edge": "res://art/image_7.png",
}

const OVERLAY_LIBRARY := {
	"scan": {
		"label": "SCAN OVERLAY",
		"note": "Field telemetry rises over the frame. The procedure is not active yet.",
	},
	"target_lock": {
		"label": "TARGET LOCK",
		"note": "The room narrows around the cut point. Advance again to commit the handoff.",
	},
}

const SURGERY_OUTCOME_LABELS := {
	"12A": "OUTCOME_12A",
	"12B": "OUTCOME_12B",
	"12C": "OUTCOME_12C",
	"12D": "OUTCOME_12D",
}
const PHASE_SCRIPT_PATHS := {
	"F1": F1_SCRIPT_PATH,
	"F2": F2_SCRIPT_PATH,
	"F3": F3_SCRIPT_PATH,
}
const SURGERY_OUTCOME_LABELS_BY_PHASE := {
	"F1": SURGERY_OUTCOME_LABELS,
	"F2": {
		"12A": "F2_OUTCOME_A",
		"12B": "F2_OUTCOME_B",
		"12C": "F2_OUTCOME_C",
		"12D": "F2_OUTCOME_B",
	},
	"F3": {
		"12A": "F3_OUTCOME_A",
		"12B": "F3_OUTCOME_B",
		"12C": "F3_OUTCOME_C",
		"12D": "F3_OUTCOME_N",
	},
}
const F2_VICTORIA_LINE_LABELS := {
	"12A": "F2_VIK_LINE_A",
	"12B": "F2_VIK_LINE_B",
	"12C": "F2_VIK_LINE_C",
	"12D": "F2_VIK_LINE_B",
}

const SAMPLE_SEQUENCE_BY_PHASE := {
	"F1": """::step
[img: image_1]
РАССКАЗЧИК:
Вейр выбрасывает протуберанцы вверх — на сотни метров, иногда на километры.

::step
[img: image_2]
РАССКАЗЧИК:
Так он «охотится» на живых.

::step
[img: image_3]
РАССКАЗЧИК:
Хотя охотиться он, конечно, не может. Это просто свойство.

::step
[img: image_4]
ДЕЗМОНД:
Это… чёрт меня дери… оно сработало!

::step
[octavia: Красиво упал. Хотя я бы ронял ближе к центру.]

::step
ДЕЗМОНД:
И вопрос теперь — что с этим делать?.. И где я.
[img: image_4-2.png]

::pan

::step
РАССКАЗЧИК:
Но довольно философии. Давайте следить за героем нашего фильма.

::step
[img: image_5]
ДЕЙСТВИЕ:
ДЕЗМОНД осторожно встаёт.
Тело рядом дёргается, высекая из сетки искры.
Он приседает, ловит искру в воздухе — и тут же одёргивает руку, будто обжигается.

::step
[octavia: Слишком ровно происходит… Стоп. Что?]

::step
КРУПНО:
Под его шагами искры не вылетают.

[octavia: Я вообще не должен этого видеть! А меня волнует, что всё слишком… Ровно?]

::step
ДЕЙСТВИЕ:
ДЕЗМОНД подходит к мостику.
Переходит на соседнюю платформу.
У мостика стоит столбик с небольшим пультом или датчиком.
[img: image_6]

::step
ДЕЗМОНД (ТИХО):
Ноль реакции. И искр — ноль. Оттого, что я — не отсюда, да.

::step
ДЕЙСТВИЕ:
На пульте горит красный огонёк.
ДЕЗМОНД машет рукой рядом, потом касается. Ничего.

::step
ДЕЗМОНД (ТИХО):
Здесь не жарко и не влажно. Дымка висит в метре над полом — явно не пар и не туман...

::step
[img: image_7]
ВИЗУАЛ:
Там, где нет платформ, нет и дымки.
Видно широкое плоское чёрное «крыло» — ромбовидное, с фиолетовыми огнями по краям. Оно удаляется.
[octavia: И вот это, это — я, но… это ведь уже произошло около часа назад…]

::step
[img: image_8]
ЗВУК:
Тяжёлые, уверенные, бездушные шаги.

ДЕЙСТВИЕ:
ДЕЗМОНД замечает лестницу на противоположном конце платформы.
Лестница уходит вверх, в дымку. ДЕЗМОНД прячется на ней.

ДЕЙСТВИЕ:
На этой платформе — такие же сетчатые полы, дымка в метре над ними, массивные угловатые механизмы с фиолетовыми искрами внутри.
И ещё — люди.

КРУПНЕЕ:
И не-люди.

::step
[img: image_9]

::pan

::step
ДЕЗМОНД:
Ох ты ж, храни меня святой Шелдон!..

[octavia: Это повтор. Почему это повтор?]

ДЕЙСТВИЕ:
Люди спокойно и организованно идут к дальней стороне платформы, откуда слышны шаги.
Автоматоны — нет.

КРУПНО:
Под ногами людей из пола вылетают фиолетовые псевдо-искры.
Под ногами автоматонов — нет, под ногами Дезмонда — тоже нет.

::step
[img: image_10]

ДЕЙСТВИЕ:
Источник шагов спускается по той же лестнице, по которой поднимаются люди.

ВИЗУАЛ:
Это боевой автоматон.
Широкий, блестящий, с очень человеческим на вид ружьём.

ЗВУК / ДЕЙСТВИЕ:
Где-то вдали раздаётся выстрел.
Из груди автоматона высекаются обычные искры — металлом о металл.
Он слегка пошатывается.

::step
[img: image_11]
КРУПНО:
И ничего больше.

::step
ДЕЗМОНД (ТИХО):
Кто бы это ни был, нужно помочь.

::step
ДЕЙСТВИЕ:
На Дезмонда не обращают внимания.
Он присматривается: сквозь дымку видно фигуру в красном и фигуру в сером.

[octavia: Ну вот для начала, с чего он взял, что стреляли — хорошие ребята, а не наоборот?]

::step
ДЕЙСТВИЕ:
ДЕЗМОНД замечает на краю платформы аварийный ящик.
По автоматону ещё пару раз безуспешно стреляют.
ДЕЗМОНД перебегает к ящику.

КРУПНО:
Внутри — сигнальный пистолет. Или что-то очень на него похожее.

::step
[img: image_12]

::pan

::step
ДЕЙСТВИЕ:
Автоматон делает тяжёлый ровный шаг и вскидывает ружьё.
ДЕЗМОНД целится не в него, а в пол под его ногами.

ДЕЗМОНД (ТИХО):
Ну вот с чего я вообще взял…

[octavia: …Это я делаю?]

::step 
::goto: SURGERY MODE

::label: OUTCOME_12A

::step
[img: image_13]
::turn

::step
ДЕЙСТВИЕ:
Ракета взрывается под ногами автоматона. Он теряет равновесие.

::step
Выстрел с той стороны попадает туда же — под ноги.

::step
Сетка надрывается.

::step
[img: image_14]
ДЕЙСТВИЕ:
Автоматон проваливается в разрыв, выпускает ружьё и по-человечески хватается за край сетки.

::step
::goto: POST_OUTCOME_COMMON

::label: OUTCOME_12B

::step
[img: image_15]
::turn

::step
ДЕЙСТВИЕ:
Пистолет оказывается сигнальным — и это кстати. Ракета отскакивает от пола и взрывается под потолком следующего этажа, в дымке.

::step
Дымка мгновенно зеленеет, опускается до пола и становится непроглядной.

::step
ЗВУК:
С той стороны звучит несколько выстрелов.

ДЕЙСТВИЕ:
Автоматону они не вредят, но сбивают крепление ружья.
Ружьё падает на пол.

::step
[img: image_16]
ДЕЙСТВИЕ:
ДЕЗМОНД кидается автоматону под ноги, подхватывает ружьё и бежит прочь.

ЗВУК:
Где-то вдали раздаётся женский вскрик.

::step
::goto: POST_OUTCOME_COMMON

::label: OUTCOME_12C

::step
[img: image_14]
::turn

::step
ДЕЙСТВИЕ:
Они стреляют одновременно.

::step
Отдача и удар в сетку роняют автоматона. Он проваливается в разрыв, выпускает ружьё, хватается за сетку.

::step
Сетка рвётся, автоматон летит вниз. Ружьё вылетает из рук и скользит к ДЕЗМОНДУ.

::step
[img: image_18]
[octavia: Как в дрянном синематике стоит...]

::step
::pan

::step
КРУПНО:
Все, кто остался на платформе, оборачиваются к нему.

::step
ЗВУК / ДЕЙСТВИЕ:
Снизу лязг — автоматон упал на платформу ниже. ДЕЗМОНД подхватывает оружие и бросается наутёк.

ЗВУК:
Где-то вдали раздаётся женский вскрик.

::step
::goto: POST_OUTCOME_COMMON

::label: OUTCOME_12D

::step
[img: image_19]
::turn

::step
[octavia: Вот! Опять это «слишком гладко!»]

::step
ДЕЙСТВИЕ:
Мир на долю секунды блекнет. Потом вокруг вспыхивает форменное светопреставление.

::step
Пуля с той стороны попадает в ракету. Та взрывается в воздухе, рассыпаясь фиолетовыми не-искрами.

::step
Автоматон начинает судорожно дёргаться и роняет ружьё.

::step
[img: image_16]
ДЕЙСТВИЕ:
ДЕЗМОНД кидается к оружию, подхватывает его и бросается наутёк. Где-то вдали слышен женский вскрик.

::step
::goto: POST_OUTCOME_COMMON

::label: POST_OUTCOME_COMMON
::step
::goto: SNAPSHOT MODE""",
	"F2": """[phase: F2]
[img: theater_hold]
[octavia: F2 sample sequence is active until authored content replaces it.]

The shell reopens carrying the prior totals.
::step
::goto: SURGERY MODE""",
	"F3": """[phase: F3]
[img: resolution_edge]
[octavia: Last playable beat. One more surgery pass resolves the run.]

The archive edge stays stable just long enough for one final routing step.
::step
::goto: SURGERY MODE""",
}

@onready var scene_image_area: PanelContainer = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea
@onready var scene_image_texture: TextureRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/SceneImageTexture
@onready var scene_image_label: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/SceneImageLabel
@onready var scene_image_note: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/SceneImageNote
@onready var oblivion_event_overlay: TextureRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/OblivionEventOverlay
@onready var burn_event_overlay: TextureRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/BurnEventOverlay
@onready var inner_voice_layer: Control = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/OctaviusInnerVoiceLayer
@onready var inner_voice_label: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/OctaviusInnerVoiceLayer/OctaviusInnerVoiceLabel
@onready var scene_frame: PanelContainer = $Margin/RootStack/MainRow/SceneFrame
@onready var frame_canvas: Control = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas
@onready var filmstrip_left: TextureRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/FilmstripLeft
@onready var filmstrip_right: TextureRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/FilmstripRight
@onready var fade_overlay: ColorRect = $FadeOverlay
@onready var pan_prompt_label: Label = $PanPromptLabel
@onready var turn_interference_a: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/TurnInterferenceA
@onready var turn_interference_b: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/TurnInterferenceB
@onready var portrait_strip: PanelContainer = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip
@onready var portrait_bleach_overlay: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/BleachOverlay
@onready var portrait_oblivion_static_a: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/OblivionStaticA
@onready var portrait_oblivion_static_b: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/OblivionStaticB
@onready var portrait_oblivion_grime_left: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/OblivionGrimeLeft
@onready var portrait_oblivion_grime_right: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/OblivionGrimeRight
@onready var portrait_pressure_edge_top: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/PressureEdgeTop
@onready var portrait_pressure_edge_bottom: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/PressureEdgeBottom
@onready var portrait_pressure_fracture_left: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/PressureFractureLeft
@onready var portrait_pressure_fracture_right: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitFx/PressureFractureRight
@onready var dossier_slot_a: PanelContainer = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotA
@onready var dossier_slot_a_headshot_label: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotA/DossierSlotAMargin/DossierSlotAStack/DossierSlotAHeadshot/DossierSlotAHeadshotLabel
@onready var dossier_slot_a_title: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotA/DossierSlotAMargin/DossierSlotAStack/DossierSlotATitle
@onready var dossier_slot_a_cue: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotA/DossierSlotAMargin/DossierSlotAStack/DossierSlotACue
@onready var dossier_slot_b: PanelContainer = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotB
@onready var dossier_slot_b_headshot_label: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotB/DossierSlotBMargin/DossierSlotBStack/DossierSlotBHeadshot/DossierSlotBHeadshotLabel
@onready var dossier_slot_b_title: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotB/DossierSlotBMargin/DossierSlotBStack/DossierSlotBTitle
@onready var dossier_slot_b_cue: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotB/DossierSlotBMargin/DossierSlotBStack/DossierSlotBCue
@onready var dossier_slot_c: PanelContainer = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotC
@onready var dossier_slot_c_headshot_label: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotC/DossierSlotCMargin/DossierSlotCStack/DossierSlotCHeadshot/DossierSlotCHeadshotLabel
@onready var dossier_slot_c_title: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotC/DossierSlotCMargin/DossierSlotCStack/DossierSlotCTitle
@onready var dossier_slot_c_cue: Label = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/PortraitStrip/PortraitMargin/PortraitRow/DossierSlotC/DossierSlotCMargin/DossierSlotCStack/DossierSlotCCue
@onready var frame_damage_top: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/FrameDamageTop
@onready var frame_damage_right: ColorRect = $Margin/RootStack/MainRow/SceneFrame/FrameMargin/FrameCanvas/FrameDamageRight

@onready var overline: Label = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/Overline
@onready var title_label: Label = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/Title
@onready var beat_line: Label = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/BeatLine
@onready var body_copy: RichTextLabel = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/BodyCopy
@onready var cue_card: PanelContainer = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/CueCard
@onready var cue_card_text: Label = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/CueCard/CueCardMargin/CueCardText
@onready var scratch_notes: Label = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/ScratchNotes
@onready var action_buttons: VBoxContainer = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/ActionButtons
@onready var primary_action_button: Button = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/ActionButtons/PrimaryActionButton
@onready var secondary_action_button: Button = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/ActionButtons/SecondaryActionButton
@onready var tertiary_action_button: Button = $Margin/RootStack/MainRow/ScriptColumn/ScriptMargin/ScriptStack/ActionButtons/TertiaryActionButton
@onready var surgery_overlay: Control = $SurgeryOverlay
@onready var overlay_label: Label = $SurgeryOverlay/OverlayShade/OverlayLabel
@onready var overlay_note: Label = $SurgeryOverlay/OverlayShade/OverlayNote

var current_phase := "F1"
var metrics_pulse_time := 0.0
var authored_steps: Array[Dictionary] = []
var authored_label_lookup := {}
var current_step_index := 0
var pending_octavia_clear_clicks := -1
var is_fading_in := true
var narrative_progression_locked := false
var is_transitioning := false
var turn_phase_index := 0
var turn_clicks_remaining := 0
var turn_resume_ready := false
var is_pan_active := false
var active_pan_tween: Tween
var active_fade_tween: Tween
var active_body_copy_emphasis_tween: Tween
var active_event_fx_tween: Tween
var layout_refresh_queued := false
var active_branch_end_step_index := -1
var pending_initial_step_index := -1
var pending_initial_branch_end_step_index := -1
var active_event_fx := EVENT_FX_NONE
var filmstrip_left_offset := 0.0
var filmstrip_right_offset := 0.0
var filmstrip_left_tiles: Array[TextureRect] = []
var filmstrip_right_tiles: Array[TextureRect] = []
var last_requested_phase_id := ""
var last_resolved_phase_id := ""
var last_resolved_script_path := ""
var last_script_source_kind := "builtin_sample"
var last_script_used_fallback := false


func _ready() -> void:
	if has_node("/root/TitleMusic"):
		TitleMusic.ensure_title_theme()
	_resolve_phase()
	_bind_actions()
	_setup_filmstrip_loops()
	_configure_display_channels()
	_refresh_phase_headers()
	_load_authored_sequence()
	_refresh_metric_panel()
	fade_overlay.show()
	fade_overlay.color = Color(0, 0, 0, 1)
	set_process(true)
	call_deferred("_finish_initial_layout")


func _notification(what: int) -> void:
	if what == NOTIFICATION_RESIZED:
		_queue_frame_layout_refresh()


func _process(delta: float) -> void:
	if not _can_run_runtime_updates():
		return

	_update_filmstrip_loops(delta)
	metrics_pulse_time += delta
	_refresh_metric_panel()
	_animate_continue_prompt()


func _input(event: InputEvent) -> void:
	if not _is_input_available():
		return

	if is_fading_in:
		return

	if is_pan_active:
		return

	if _is_turn_consuming_input():
		if event.is_action_pressed("ui_accept"):
			_advance_turn_sequence()
			_mark_input_handled()
			return

		if event is InputEventMouseButton:
			var turn_mouse_event := event as InputEventMouseButton
			if turn_mouse_event.button_index == MOUSE_BUTTON_LEFT and turn_mouse_event.pressed:
				_advance_turn_sequence()
				_mark_input_handled()
				return

	if narrative_progression_locked:
		return

	if event.is_action_pressed("ui_accept"):
		_debug_log_step_advance()
		_advance_authored_step()
		_mark_input_handled()
		return

	if event is InputEventMouseButton:
		var mouse_event := event as InputEventMouseButton
		if mouse_event.button_index == MOUSE_BUTTON_LEFT and mouse_event.pressed:
			_debug_log_step_advance()
			_advance_authored_step()
			_mark_input_handled()


func _resolve_phase() -> void:
	if _has_game_state():
		var pending_resume: Dictionary = GameState.peek_gameplay_resume()
		var resume_phase := str(pending_resume.get("phase", ""))
		if not resume_phase.is_empty() and PHASE_FLOW.has(resume_phase):
			current_phase = resume_phase
			_log_phase_transition("resolve_phase.resume", current_phase)
			return

		GameState.ensure_runtime_phase()
		current_phase = GameState.current_phase
	if not PHASE_FLOW.has(current_phase):
		current_phase = PHASE_FLOW[0]
	_log_phase_transition("resolve_phase.runtime", current_phase)


func _bind_actions() -> void:
	action_buttons.hide()
	primary_action_button.hide()
	secondary_action_button.text = "Return to Title"
	tertiary_action_button.hide()


func _configure_display_channels() -> void:
	overline.hide()
	title_label.hide()
	beat_line.hide()
	cue_card.hide()
	scratch_notes.hide()
	body_copy.text = ""
	body_copy.clear()
	_reset_body_copy_emphasis()
	body_copy.scroll_to_line(0)
	_clear_overlay()
	_clear_octavia()
	_style_octavia_label()
	_style_continue_prompt()
	_style_metric_panel()
	_hide_continue_prompt()
	_clear_turn_interference()
	_reset_event_fx_state()


func _refresh_phase_headers() -> void:
	overline.text = ""
	title_label.text = ""
	beat_line.text = ""


func _load_authored_sequence() -> void:
	print("[GameplayScreen] content_request requested_phase=", current_phase)
	var script_text := _get_authored_script_text(current_phase)
	var parsed_script := _parse_steps(script_text)
	authored_steps.clear()
	for step_data: Variant in parsed_script.get("steps", []):
		if step_data is Dictionary:
			authored_steps.append(step_data)

	authored_label_lookup = {}
	var parsed_labels: Variant = parsed_script.get("labels", {})
	if parsed_labels is Dictionary:
		authored_label_lookup = parsed_labels

	print(
		"[GameplayScreen] content_load requested_phase=", last_requested_phase_id,
		" resolved_phase=", last_resolved_phase_id,
		" resolved_script_path=", last_resolved_script_path if not last_resolved_script_path.is_empty() else "<builtin_sample>",
		" source_kind=", last_script_source_kind,
		" fallback_used=", last_script_used_fallback,
		" parsed_steps=", authored_steps.size(),
		" parsed_labels=", authored_label_lookup.size()
	)

	current_step_index = 0
	active_branch_end_step_index = -1
	pending_initial_step_index = -1
	pending_initial_branch_end_step_index = -1
	narrative_progression_locked = false
	_reset_turn_state(true)
	_log_parsed_step_preview()
	_log_authored_labels()
	_apply_pending_resume_state()

	if authored_steps.is_empty():
		push_warning(
			"GameplayScreen: authored sequence parsed zero steps for requested phase '%s' (resolved phase '%s', path '%s', fallback=%s)" % [
				last_requested_phase_id,
				last_resolved_phase_id,
				last_resolved_script_path,
				str(last_script_used_fallback),
			]
		)
		authored_steps = [{"lines": ["[i]No authored steps found.[/i]"], "commands": []}]

	if pending_initial_step_index < 0:
		_apply_default_image_for_phase()
	else:
		print("[GameplayScreen] deferred initial image for pending resume step=", pending_initial_step_index)
	_clear_octavia()


func _get_authored_script_text(phase_id: String) -> String:
	last_requested_phase_id = phase_id.strip_edges()
	last_resolved_phase_id = last_requested_phase_id
	last_resolved_script_path = ""
	last_script_source_kind = "builtin_sample"
	last_script_used_fallback = false

	if not PHASE_SCRIPT_PATHS.has(last_resolved_phase_id):
		push_warning("GameplayScreen: unknown requested phase '%s' for content load; falling back to F1" % last_requested_phase_id)
		last_resolved_phase_id = "F1"
		last_script_used_fallback = true

	var raw_text := SAMPLE_SEQUENCE_BY_PHASE.get(last_resolved_phase_id, SAMPLE_SEQUENCE_BY_PHASE["F1"]) as String
	var script_path := str(PHASE_SCRIPT_PATHS.get(last_resolved_phase_id, ""))
	last_resolved_script_path = script_path
	if script_path.is_empty():
		last_script_source_kind = "builtin_sample"
		last_script_used_fallback = true
		push_warning("GameplayScreen: missing script path for phase '%s'; using sample content" % last_resolved_phase_id)
	else:
		var file_exists := FileAccess.file_exists(script_path)
		if not file_exists:
			last_script_source_kind = "builtin_sample"
			last_script_used_fallback = true
			push_warning("GameplayScreen: authored script file missing for phase '%s' at %s; using sample content" % [last_resolved_phase_id, script_path])
		else:
			var file := FileAccess.open(script_path, FileAccess.READ)
			if file == null:
				last_script_source_kind = "builtin_sample"
				last_script_used_fallback = true
				push_warning("GameplayScreen: authored script file could not be opened for phase '%s' at %s; using sample content" % [last_resolved_phase_id, script_path])
			else:
				var file_text := file.get_as_text()
				if _normalize_script_text(file_text).is_empty():
					last_script_source_kind = "builtin_sample"
					last_script_used_fallback = true
					push_warning("GameplayScreen: authored script file is empty for phase '%s' at %s; using sample content" % [last_resolved_phase_id, script_path])
				else:
					last_script_source_kind = "file"
					raw_text = file_text

	print(
		"[GameplayScreen] content_source requested_phase=", last_requested_phase_id,
		" resolved_phase=", last_resolved_phase_id,
		" resolved_script_path=", script_path if not script_path.is_empty() else "<none>",
		" source_kind=", last_script_source_kind,
		" fallback_used=", last_script_used_fallback
	)
	print("[GameplayScreen] raw_text_length=", raw_text.length())
	print("[GameplayScreen] raw_preview=", JSON.stringify(_preview_text(raw_text)))
	var normalized_text := _normalize_script_text(raw_text)
	print("[GameplayScreen] normalized_text_length=", normalized_text.length())
	print("[GameplayScreen] normalized_preview=", JSON.stringify(_preview_text(normalized_text)))
	return normalized_text


func _preview_text(text: String, max_chars: int = 120) -> String:
	return text.substr(0, mini(text.length(), max_chars))


func _normalize_script_text(text: String) -> String:
	var normalized_text := text.replace("\r\n", "\n").replace("\r", "\n")
	if normalized_text.begins_with("\ufeff"):
		normalized_text = normalized_text.substr(1)
	return normalized_text


func _show_initial_step() -> void:
	if pending_initial_step_index >= 0:
		current_step_index = pending_initial_step_index
		active_branch_end_step_index = pending_initial_branch_end_step_index
		print("[GameplayScreen] presenting deferred resume step=", pending_initial_step_index, " branch_end_step_index=", pending_initial_branch_end_step_index)
		pending_initial_step_index = -1
		pending_initial_branch_end_step_index = -1

	if authored_steps.is_empty() or current_step_index >= authored_steps.size():
		return

	var step: Dictionary = authored_steps[current_step_index]
	current_step_index += 1
	_apply_step(step)
	_refresh_continue_prompt()


func _parse_steps(script_text: String) -> Dictionary:
	var parsed_steps: Array[Dictionary] = []
	var label_lookup := {}
	var pending_labels: Array[String] = []
	var step_started := false
	var step_commands: Array[Dictionary] = []
	var line_command_regex := RegEx.new()
	line_command_regex.compile("^:+([A-Za-z_]+)(?:\\s*:\\s*(.+))?$")
	var step_text_builder := ""

	for raw_line: String in script_text.split("\n", false):
		var line := raw_line.strip_edges()
		if _is_authored_control_line(line):
			var match := line_command_regex.search(line)
			if match != null:
				var command_name := match.get_string(1).to_lower()
				var command_value := match.get_string(2).strip_edges()
				match command_name:
					"step":
						if step_started:
							_append_parsed_step(parsed_steps, step_text_builder, step_commands)
							step_commands.clear()
							step_text_builder = ""

						var target_step_index := parsed_steps.size()
						for pending_label: String in pending_labels:
							if label_lookup.has(pending_label):
								push_warning("GameplayScreen: duplicate authored label '%s'" % pending_label)
								continue
							label_lookup[pending_label] = target_step_index

						pending_labels.clear()
						step_started = true
					"label":
						var normalized_label := _normalize_authored_label(command_value)
						if normalized_label.is_empty():
							push_warning("GameplayScreen: empty authored label declaration")
						else:
							pending_labels.append(normalized_label)
					_:
						if not step_started:
							step_started = true
						step_commands.append({
							"name": command_name,
							"value": command_value,
						})
				continue
			push_warning("GameplayScreen: malformed authored control line '%s'" % line)
			continue

		if not step_started:
			step_started = true
		var inline_data := _extract_inline_commands(raw_line)
		var visible_text := str(inline_data.get("text", ""))
		if not step_text_builder.is_empty():
			step_text_builder += "\n"
		step_text_builder += visible_text
		for command_data: Variant in inline_data.get("commands", []):
			if command_data is Dictionary:
				step_commands.append(command_data)

	if step_started:
		_append_parsed_step(parsed_steps, step_text_builder, step_commands)
	if not pending_labels.is_empty():
		push_warning("GameplayScreen: labels without following step ignored: %s" % ", ".join(pending_labels))

	print("[GameplayScreen] parsed total_steps=", parsed_steps.size())
	print("[GameplayScreen] parsed labels=", label_lookup)
	if parsed_steps.is_empty():
		_log_zero_step_diagnostics(script_text)

	return {
		"steps": parsed_steps,
		"labels": label_lookup,
	}


func _append_parsed_step(parsed_steps: Array[Dictionary], step_text_builder: String, step_commands: Array[Dictionary]) -> void:
	var step_lines: Array[String] = []
	var visible_text := step_text_builder.strip_edges()
	if not visible_text.is_empty():
		step_lines.append(visible_text)

	var merged_commands: Array[Dictionary] = []
	for command_data: Dictionary in step_commands:
		merged_commands.append(command_data)

	if merged_commands.is_empty() and step_lines.is_empty():
		return

	parsed_steps.append({
		"lines": step_lines,
		"commands": merged_commands,
	})


func _is_authored_control_line(line: String) -> bool:
	if line.is_empty() or not line.begins_with(":"):
		return false

	var normalized_line := line.lstrip(":").strip_edges()
	if normalized_line.is_empty():
		return false

	var command_name := normalized_line.split(":", true, 1)[0].strip_edges().to_lower()
	return command_name in [
		"step",
		"label",
		"goto",
		"pan",
		"turn",
		"phase",
		"img",
		"overlay",
		"octavia",
		"clear_overlay",
		"clear_octavia",
	]


func _extract_inline_commands(step_text: String) -> Dictionary:
	var commands: Array[Dictionary] = []
	var visible_parts: Array[String] = []
	var cursor := 0
	var command_regex := RegEx.new()
	command_regex.compile("\\[(img|overlay|octavia|goto|phase)\\s*:\\s*([^\\]]*)\\]|\\[(clear_overlay|clear_octavia)\\]")

	for match: RegExMatch in command_regex.search_all(step_text):
		var start := match.get_start()
		if start > cursor:
			visible_parts.append(step_text.substr(cursor, start - cursor))

		var command_name := ""
		var command_value := ""
		if match.get_string(1) != "":
			command_name = match.get_string(1)
			command_value = match.get_string(2).strip_edges()
		else:
			command_name = match.get_string(3)

		commands.append({
			"name": command_name,
			"value": command_value,
		})
		cursor = match.get_end()

	if cursor < step_text.length():
		visible_parts.append(step_text.substr(cursor))

	var visible_text := "".join(visible_parts)
	return {
		"text": visible_text,
		"commands": commands,
	}


func _advance_authored_step() -> void:
	if not _is_runtime_active() or narrative_progression_locked or current_step_index >= authored_steps.size():
		return

	if active_branch_end_step_index >= 0 and current_step_index >= active_branch_end_step_index:
		print("[GameplayScreen] branch playback complete at step=", current_step_index, " boundary=", active_branch_end_step_index)
		var branch_resume_step_index := active_branch_end_step_index
		active_branch_end_step_index = -1
		if _should_show_snapshot_after_branch():
			if _has_game_state():
				GameState.set_gameplay_resume(current_phase, branch_resume_step_index)
			_begin_scene_transition(SNAPSHOT_SCENE_PATH)
			return
		current_step_index = authored_steps.size()
		return

	_clear_turn_visuals_if_pending()
	_decay_octavia()

	var step: Dictionary = authored_steps[current_step_index]
	current_step_index += 1
	_apply_step(step)

	_refresh_metric_panel()
	_refresh_continue_prompt()


func _apply_step(step: Dictionary) -> void:
	if not _is_runtime_active():
		return

	_hide_continue_prompt()
	var raw_commands: Variant = step.get("commands", [])
	var commands: Array[Dictionary] = _build_command_array(raw_commands)
	var raw_lines: Variant = step.get("lines", [])
	var lines: Array[String] = _build_string_array(raw_lines)
	print("[GameplayScreen] _apply_step step_index=", maxi(current_step_index - 1, 0), " lines_type=", type_string(typeof(raw_lines)), " commands_type=", type_string(typeof(raw_commands)), " line_count=", lines.size(), " command_count=", commands.size())
	for command_idx: int in range(commands.size()):
		var command_data: Dictionary = commands[command_idx]
		print("[GameplayScreen] _apply_step command ", command_idx, " name=", str(command_data.get("name", "")), " value=", str(command_data.get("value", "")))

	for command_data: Dictionary in commands:
		_execute_command(command_data)
		if narrative_progression_locked:
			_refresh_continue_prompt()
			return

	if not lines.is_empty():
		_append_body_text("\n".join(lines))
	_refresh_continue_prompt()


func _execute_command(command_data: Dictionary) -> void:
	var command_name := str(command_data.get("name", ""))
	var command_value := str(command_data.get("value", ""))
	print("[GameplayScreen] executing command name=", command_name, " value=", command_value)

	match command_name:
		"img":
			_apply_image(command_value)
		"overlay":
			_apply_overlay(command_value)
		"clear_overlay":
			_clear_overlay()
		"octavia":
			_set_octavia_text(command_value)
		"clear_octavia":
			_clear_octavia()
		"goto":
			_handle_goto_command(command_value)
		"label":
			_handle_label_command(command_value)
		"pan":
			_begin_pan(command_value)
		"turn":
			_begin_turn_sequence()
		"phase":
			_set_phase(command_value)
		_:
			push_warning("GameplayScreen: unsupported authored command '%s'" % command_name)


func _handle_label_command(_label_name: String) -> void:
	return


func jump_to_label(label_name: String) -> bool:
	var normalized_label := _normalize_authored_label(label_name)
	if normalized_label.is_empty():
		push_warning("GameplayScreen: cannot jump to empty label")
		return false

	if not authored_label_lookup.has(normalized_label):
		push_warning("GameplayScreen: unknown authored label '%s'" % normalized_label)
		return false

	current_step_index = int(authored_label_lookup.get(normalized_label, 0))
	active_branch_end_step_index = _resolve_branch_end_step_index(current_step_index)
	narrative_progression_locked = false
	_handle_event_fx_label(normalized_label)
	_refresh_continue_prompt()
	print("[GameplayScreen] jump_to_label label=", normalized_label, " target_step_index=", current_step_index, " branch_end_step_index=", active_branch_end_step_index)
	return true


func _resolve_branch_end_step_index(target_step_index: int) -> int:
	var sorted_targets: Array[int] = []
	for mapped_step_index: Variant in authored_label_lookup.values():
		sorted_targets.append(int(mapped_step_index))
	sorted_targets.sort()

	for branch_start: int in sorted_targets:
		if branch_start > target_step_index:
			return branch_start

	return authored_steps.size()


func _resolve_outcome_label(outcome_id: String) -> String:
	var phase_labels: Dictionary = SURGERY_OUTCOME_LABELS_BY_PHASE.get(current_phase, SURGERY_OUTCOME_LABELS)
	return str(phase_labels.get(outcome_id.strip_edges().to_upper(), ""))


func _log_authored_labels() -> void:
	var label_names: Array[String] = []
	for label_name: Variant in authored_label_lookup.keys():
		label_names.append(str(label_name))
	label_names.sort()

	for label_name: String in label_names:
		print("[GameplayScreen] parsed label ", label_name, " -> step ", int(authored_label_lookup.get(label_name, -1)))


func _log_parsed_step_preview() -> void:
	var preview_count := mini(authored_steps.size(), 5)
	for step_idx: int in range(preview_count):
		var step_data: Dictionary = authored_steps[step_idx]
		var lines: Array[String] = _build_string_array(step_data.get("lines", []))
		var commands: Array[Dictionary] = _build_command_array(step_data.get("commands", []))
		var labels: Array[String] = _labels_for_step_index(step_idx)
		print("[GameplayScreen] parsed step preview index=", step_idx, " line_count=", lines.size(), " command_count=", commands.size(), " labels=", labels)


func _log_zero_step_diagnostics(script_text: String) -> void:
	var step_token_present := script_text.find(STEP_SEPARATOR) != -1
	var split_parts := script_text.split(STEP_SEPARATOR, false)
	print("[GameplayScreen] zero-step diagnostics step_token_present=", step_token_present, " step_split_parts=", split_parts.size())
	if not split_parts.is_empty():
		var preview_parts: Array[String] = []
		for idx: int in range(mini(split_parts.size(), 3)):
			preview_parts.append(_preview_text(split_parts[idx], 80))
		print("[GameplayScreen] zero-step diagnostics split_preview=", preview_parts)


func _labels_for_step_index(step_idx: int) -> Array[String]:
	var labels: Array[String] = []
	for label_name: Variant in authored_label_lookup.keys():
		if int(authored_label_lookup.get(label_name, -1)) == step_idx:
			labels.append(str(label_name))
	labels.sort()
	return labels


func _build_string_array(value: Variant) -> Array[String]:
	var typed_lines: Array[String] = []
	if value is Array:
		for entry: Variant in value:
			typed_lines.append(str(entry))
	return typed_lines


func _build_command_array(value: Variant) -> Array[Dictionary]:
	var typed_commands: Array[Dictionary] = []
	if value is Array:
		for entry: Variant in value:
			if entry is Dictionary:
				typed_commands.append(entry)
	return typed_commands


func _append_body_text(text_block: String) -> void:
	if body_copy.text.is_empty():
		body_copy.text = text_block
	else:
		body_copy.text = "%s\n\n%s" % [text_block, body_copy.text]
	body_copy.scroll_to_line(0)
	_play_body_copy_emphasis()


func _reset_body_copy_emphasis() -> void:
	if is_instance_valid(active_body_copy_emphasis_tween):
		active_body_copy_emphasis_tween.kill()
	active_body_copy_emphasis_tween = null
	body_copy.position.y = 0.0
	body_copy.modulate = Color(1, 1, 1, 1)


func _play_body_copy_emphasis() -> void:
	_reset_body_copy_emphasis()
	body_copy.position.y = BODY_COPY_EMPHASIS_OFFSET_Y
	body_copy.modulate = Color(1, 1, 1, BODY_COPY_EMPHASIS_START_ALPHA)

	active_body_copy_emphasis_tween = create_tween()
	active_body_copy_emphasis_tween.set_parallel(true)
	active_body_copy_emphasis_tween.set_trans(Tween.TRANS_QUAD)
	active_body_copy_emphasis_tween.set_ease(Tween.EASE_OUT)
	active_body_copy_emphasis_tween.tween_property(body_copy, "position:y", 0.0, BODY_COPY_EMPHASIS_DURATION)
	active_body_copy_emphasis_tween.tween_property(body_copy, "modulate", Color(1, 1, 1, 1), BODY_COPY_EMPHASIS_DURATION)


func _apply_default_image_for_phase() -> void:
	match current_phase:
		"F2":
			_apply_image("theater_hold")
		"F3":
			_apply_image("resolution_edge")
		_:
			_apply_image("prep_bay")


func _apply_pending_resume_state() -> void:
	if not _has_game_state():
		return

	var resume_data := GameState.consume_gameplay_resume()
	if str(resume_data.get("phase", "")) != current_phase:
		if not str(resume_data.get("phase", "")).strip_edges().is_empty():
			print("[GameplayScreen] discard_resume phase=", str(resume_data.get("phase", "")), " current_phase=", current_phase, " step=", int(resume_data.get("step_index", 0)), " label=", str(resume_data.get("label", "")))
		return

	var requested_resume_label := _normalize_authored_label(str(resume_data.get("label", "")))
	if not requested_resume_label.is_empty():
		var outcome_id := ""
		if _has_game_state():
			outcome_id = str(GameState.current_outcome_id).strip_edges()
		var resolved_label := _resolve_outcome_label(outcome_id)
		if resolved_label.is_empty():
			resolved_label = requested_resume_label
		var resolved_target_step_index := int(authored_label_lookup.get(resolved_label, -1))
		print("[GameplayScreen] surgery return outcome_id=", outcome_id)
		print("[GameplayScreen] surgery return resolved_label=", resolved_label)
		print("[GameplayScreen] surgery return target_step_index=", resolved_target_step_index)
		if authored_label_lookup.has(resolved_label):
			pending_initial_step_index = resolved_target_step_index
			pending_initial_branch_end_step_index = -1
			current_step_index = resolved_target_step_index
			active_branch_end_step_index = -1
			_handle_event_fx_label(resolved_label)
			print("[GameplayScreen] queued deferred resume label=", resolved_label, " step_index=", pending_initial_step_index, " branch_end_step_index=disabled")
			_log_phase_transition("resume.branch", current_phase, current_phase, outcome_id)
			return
		push_warning("GameplayScreen: unknown authored label '%s'" % resolved_label)

	current_step_index = mini(int(resume_data.get("step_index", 0)), authored_steps.size())
	pending_initial_step_index = current_step_index
	pending_initial_branch_end_step_index = -1
	print("[GameplayScreen] queued deferred resume step_index=", pending_initial_step_index)
	active_branch_end_step_index = -1
	_log_phase_transition("resume.step", current_phase)


func _apply_image(image_id: String) -> void:
	var image_path := str(IMAGE_LIBRARY.get(image_id, ""))
	if image_path.is_empty():
		print("[GameplayScreen] missing image id=", image_id)
		push_warning("GameplayScreen: unknown image id '%s'" % image_id)
		return

	_reset_pan_state()
	var image_texture := load(image_path) as Texture2D
	scene_image_texture.texture = image_texture
	_sync_event_overlay_textures()
	scene_image_label.hide()
	scene_image_note.hide()
	print("[GameplayScreen] _apply_image id=", image_id)
	if image_texture != null:
		print("[GameplayScreen] _apply_image texture_size=", image_texture.get_size())
	else:
		print("[GameplayScreen] _apply_image texture_size=<null>")
	print("[GameplayScreen] _apply_image scene_image_area.size=", scene_image_area.size)
	print("[GameplayScreen] _apply_image scene_image_texture.size=", scene_image_texture.size)
	print("[GameplayScreen] _apply_image scene_image_texture.global_rect=", scene_image_texture.get_global_rect())


func _apply_overlay(overlay_id: String) -> void:
	var overlay_data: Dictionary = OVERLAY_LIBRARY.get(overlay_id, {})
	if overlay_data.is_empty():
		push_warning("GameplayScreen: unknown overlay id '%s'" % overlay_id)
		return

	surgery_overlay.visible = true
	overlay_label.text = str(overlay_data.get("label", overlay_id))
	overlay_note.text = str(overlay_data.get("note", ""))


func _clear_overlay() -> void:
	surgery_overlay.visible = false
	overlay_label.text = ""
	overlay_note.text = ""


func _set_octavia_text(text: String) -> void:
	inner_voice_label.text = text
	inner_voice_layer.visible = not text.is_empty()
	inner_voice_label.visible = not text.is_empty()
	inner_voice_layer.z_index = 10
	inner_voice_label.z_index = 11
	pending_octavia_clear_clicks = OCTAVIA_AUTO_CLEAR_CLICKS
	print("[GameplayScreen] _set_octavia_text text=", text)
	print("[GameplayScreen] _set_octavia_text layer_visible=", inner_voice_layer.visible)
	print("[GameplayScreen] _set_octavia_text label_visible=", inner_voice_label.visible)
	print("[GameplayScreen] _set_octavia_text layer_global_rect=", inner_voice_layer.get_global_rect())
	print("[GameplayScreen] _set_octavia_text label_global_rect=", inner_voice_label.get_global_rect())
	print("[GameplayScreen] _set_octavia_text layer_z_index=", inner_voice_layer.z_index)
	print("[GameplayScreen] _set_octavia_text label_z_index=", inner_voice_label.z_index)


func _clear_octavia() -> void:
	inner_voice_label.text = ""
	inner_voice_label.visible = false
	inner_voice_layer.visible = false
	pending_octavia_clear_clicks = -1


func _decay_octavia() -> void:
	if pending_octavia_clear_clicks < 0:
		return

	pending_octavia_clear_clicks -= 1
	if pending_octavia_clear_clicks <= 0:
		_clear_octavia()


func _style_octavia_label() -> void:
	inner_voice_label.add_theme_color_override("font_color", Color(1.0, 0.980392, 0.780392, 1.0))
	inner_voice_label.add_theme_color_override("font_shadow_color", Color(0.0, 0.0, 0.0, 0.95))
	inner_voice_label.add_theme_constant_override("shadow_offset_x", 2)
	inner_voice_label.add_theme_constant_override("shadow_offset_y", 2)
	inner_voice_label.add_theme_font_size_override("font_size", 18)
	inner_voice_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	inner_voice_layer.set_anchors_preset(Control.PRESET_FULL_RECT)
	inner_voice_label.position = Vector2(28.0, 24.0)
	inner_voice_label.size = Vector2(700.0, 96.0)
	inner_voice_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART


func _style_continue_prompt() -> void:
	cue_card.hide()
	if is_instance_valid(pan_prompt_label):
		pan_prompt_label.hide()
	cue_card_text.text = CONTINUE_PROMPT_COPY
	cue_card_text.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	cue_card_text.add_theme_font_size_override("font_size", 17)
	cue_card_text.add_theme_color_override("font_color", Color(0.356863, 0.278431, 0.196078, 0.9))


func _show_continue_prompt() -> void:
	if is_pan_active:
		_hide_continue_prompt()
		return
	cue_card.show()
	if is_instance_valid(pan_prompt_label):
		pan_prompt_label.hide()
	cue_card.modulate = Color(1.0, 1.0, 1.0, 0.98)
	cue_card.scale = Vector2.ONE


func _hide_continue_prompt() -> void:
	if is_instance_valid(cue_card):
		cue_card.hide()
		cue_card.modulate = Color(1.0, 1.0, 1.0, 0.98)
		cue_card.scale = Vector2.ONE
	if is_instance_valid(pan_prompt_label):
		pan_prompt_label.hide()


func _refresh_continue_prompt() -> void:
	if _should_show_continue_prompt():
		_show_continue_prompt()
		return
	_hide_continue_prompt()


func _should_show_continue_prompt() -> bool:
	if not _is_runtime_active():
		return false
	if is_fading_in or is_pan_active:
		return false
	if is_instance_valid(action_buttons) and action_buttons.visible:
		return false
	if _is_turn_consuming_input():
		return true
	if narrative_progression_locked:
		return false
	return current_step_index < authored_steps.size()


func _animate_continue_prompt() -> void:
	if not is_instance_valid(cue_card) or not cue_card.visible:
		return

	var pulse := 0.5 + 0.5 * sin(metrics_pulse_time * 2.3)
	cue_card.modulate = Color(1.0, 1.0, 1.0, lerpf(0.72, 0.98, pulse))
	cue_card.scale = Vector2.ONE * lerpf(0.985, 1.0, pulse)


func _style_metric_panel() -> void:
	dossier_slot_a.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	dossier_slot_b.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	dossier_slot_c.size_flags_horizontal = Control.SIZE_FILL
	dossier_slot_c.custom_minimum_size = Vector2(138.0, 64.0)
	dossier_slot_b_title.add_theme_font_size_override("font_size", 12)
	dossier_slot_b_cue.add_theme_font_size_override("font_size", 9)
	dossier_slot_c_title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	dossier_slot_c_title.add_theme_font_size_override("font_size", 28)
	dossier_slot_c_headshot_label.add_theme_font_size_override("font_size", 13)
	dossier_slot_c_cue.visible = false


func _queue_frame_layout_refresh() -> void:
	if layout_refresh_queued:
		return

	layout_refresh_queued = true
	call_deferred("_refresh_frame_layout")


func _refresh_frame_layout() -> void:
	layout_refresh_queued = false
	if not is_instance_valid(frame_canvas):
		return

	for fixed_control: Control in [filmstrip_left, filmstrip_right, scene_image_area, portrait_strip, frame_damage_top, frame_damage_right]:
		fixed_control.set_anchors_preset(Control.PRESET_TOP_LEFT)

	var canvas_size := frame_canvas.size
	if canvas_size.x <= 0.0 or canvas_size.y <= 0.0:
		return

	var portrait_height := clampf(canvas_size.y * 0.16, PORTRAIT_STRIP_MIN_HEIGHT, PORTRAIT_STRIP_BASE_HEIGHT)
	var portrait_bottom := canvas_size.y - FRAME_BOTTOM_MATTE
	var portrait_top := portrait_bottom - portrait_height
	var strip_top := FRAME_TOP_MATTE
	var strip_bottom := portrait_top - FRAME_STRIP_TO_PORTRAIT_GAP
	if strip_bottom <= strip_top + 80.0:
		portrait_height = maxf(PORTRAIT_STRIP_MIN_HEIGHT, portrait_height - ((strip_top + 80.0) - strip_bottom))
		portrait_top = portrait_bottom - portrait_height
		strip_bottom = portrait_top - FRAME_STRIP_TO_PORTRAIT_GAP

	var strip_height := maxf(80.0, strip_bottom - strip_top)
	var strip_width := _resolve_filmstrip_width(canvas_size.x)
	var viewport_left := strip_width + FRAME_SIDE_GUTTER
	var viewport_right := canvas_size.x - strip_width - FRAME_SIDE_GUTTER
	var viewport_width := maxf(0.0, viewport_right - viewport_left)

	filmstrip_left.position = Vector2(0.0, strip_top)
	filmstrip_left.size = Vector2(strip_width, strip_height)
	filmstrip_right.position = Vector2(canvas_size.x - strip_width, strip_top)
	filmstrip_right.size = Vector2(strip_width, strip_height)
	_refresh_filmstrip_loop_layouts()

	scene_image_area.position = Vector2(viewport_left, strip_top)
	scene_image_area.size = Vector2(viewport_width, strip_height)

	portrait_strip.position = Vector2(PORTRAIT_STRIP_SIDE_INSET, portrait_top)
	portrait_strip.size = Vector2(maxf(0.0, canvas_size.x - PORTRAIT_STRIP_SIDE_INSET * 2.0), portrait_height)

	inner_voice_label.position = Vector2(28.0, 24.0)
	inner_voice_label.size = Vector2(maxf(220.0, scene_image_area.size.x - 56.0), minf(96.0, maxf(56.0, scene_image_area.size.y * 0.22)))

	frame_damage_top.position = Vector2(viewport_left + 18.0, 0.0)
	frame_damage_top.size = Vector2(maxf(0.0, viewport_width + FRAME_SIDE_GUTTER * 2.0 - 36.0), 10.0)
	frame_damage_right.position = Vector2(maxf(0.0, canvas_size.x - 14.0), strip_top + 48.0)
	frame_damage_right.size = Vector2(14.0, maxf(0.0, strip_height - 70.0))
	_sync_event_overlay_textures()


func _resolve_filmstrip_width(canvas_width: float) -> float:
	var strip_width := FRAME_STRIP_BASE_WIDTH
	var max_strip_width := maxf(FRAME_STRIP_MIN_WIDTH, (canvas_width - FRAME_VIEWPORT_MIN_WIDTH - FRAME_SIDE_GUTTER * 2.0) * 0.5)
	strip_width = minf(strip_width, max_strip_width)
	return maxf(FRAME_STRIP_MIN_WIDTH, strip_width)


func _setup_filmstrip_loops() -> void:
	filmstrip_left_tiles = _create_filmstrip_tiles(filmstrip_left, false)
	filmstrip_right_tiles = _create_filmstrip_tiles(filmstrip_right, true)
	_refresh_filmstrip_loop_layouts()


func _create_filmstrip_tiles(container: TextureRect, flip_h: bool) -> Array[TextureRect]:
	var tiles: Array[TextureRect] = []
	if not is_instance_valid(container):
		return tiles

	var source_texture := container.texture
	var source_expand_mode := container.expand_mode
	var loop_stretch_mode := TextureRect.STRETCH_SCALE
	container.clip_contents = true
	container.texture = null

	for child: Node in container.get_children():
		if child is TextureRect and String(child.name).begins_with("LoopTile"):
			tiles.append(child as TextureRect)

	if tiles.is_empty():
		for tile_index in range(2):
			var tile := TextureRect.new()
			tile.name = "LoopTile%d" % tile_index
			tile.mouse_filter = Control.MOUSE_FILTER_IGNORE
			tile.expand_mode = source_expand_mode
			tile.stretch_mode = loop_stretch_mode
			tile.flip_h = flip_h
			container.add_child(tile)
			tiles.append(tile)

	for tile: TextureRect in tiles:
		tile.texture = source_texture
		tile.flip_h = flip_h
		tile.expand_mode = source_expand_mode
		tile.stretch_mode = loop_stretch_mode

	return tiles


func _refresh_filmstrip_loop_layouts() -> void:
	filmstrip_left_offset = _layout_filmstrip_tiles(filmstrip_left, filmstrip_left_tiles, filmstrip_left_offset)
	filmstrip_right_offset = _layout_filmstrip_tiles(filmstrip_right, filmstrip_right_tiles, filmstrip_right_offset)


func _update_filmstrip_loops(delta: float) -> void:
	filmstrip_left_offset = _advance_filmstrip_tiles(filmstrip_left, filmstrip_left_tiles, filmstrip_left_offset, delta)
	filmstrip_right_offset = _advance_filmstrip_tiles(filmstrip_right, filmstrip_right_tiles, filmstrip_right_offset, delta)


func _advance_filmstrip_tiles(container: TextureRect, tiles: Array[TextureRect], offset: float, delta: float) -> float:
	var content_height := _get_filmstrip_loop_height(container)
	if content_height <= 0.0:
		return offset

	offset = wrapf(offset + FILMSTRIP_LOOP_SPEED * delta, 0.0, content_height)
	_position_filmstrip_tiles(container, tiles, offset)
	return offset


func _layout_filmstrip_tiles(container: TextureRect, tiles: Array[TextureRect], offset: float) -> float:
	var content_height := _get_filmstrip_loop_height(container)
	if content_height <= 0.0:
		return offset

	offset = wrapf(offset, 0.0, content_height)
	for tile: TextureRect in tiles:
		tile.position.x = 0.0
		tile.size = Vector2(container.size.x, content_height)
	_position_filmstrip_tiles(container, tiles, offset)
	return offset


func _position_filmstrip_tiles(container: TextureRect, tiles: Array[TextureRect], offset: float) -> void:
	if tiles.size() < 2 or not is_instance_valid(container):
		return

	var content_height := _get_filmstrip_loop_height(container)
	if content_height <= 0.0:
		return

	tiles[0].position = Vector2(0.0, offset - content_height)
	tiles[1].position = Vector2(0.0, offset)


func _get_filmstrip_loop_height(container: TextureRect) -> float:
	if not is_instance_valid(container):
		return 0.0
	return maxf(container.size.y + FILMSTRIP_LOOP_OVERFLOW, container.size.y)


func _begin_pan(preset_name: String) -> void:
	if not _is_runtime_active():
		return

	var preset := _resolve_pan_preset(preset_name)
	narrative_progression_locked = true
	_reset_pan_state()
	is_pan_active = true
	_hide_continue_prompt()

	scene_image_texture.pivot_offset = scene_image_area.size * 0.5
	scene_image_texture.scale = preset["start_scale"]
	scene_image_texture.position = preset["start_offset"]

	active_pan_tween = create_tween()
	active_pan_tween.set_parallel(false)
	active_pan_tween.set_trans(Tween.TRANS_SINE)
	active_pan_tween.set_ease(Tween.EASE_IN_OUT)
	var pan_offsets := [
		preset["start_offset"] + Vector2(-PAN_JITTER_AMPLITUDE * 0.55, 8.0),
		preset["start_offset"] + Vector2(PAN_JITTER_AMPLITUDE * 0.80, -10.0),
		preset["end_offset"] + Vector2(-PAN_JITTER_AMPLITUDE * 0.35, 12.0),
		preset["end_offset"] + Vector2(12.0, -6.0),
		preset["end_offset"],
	]
	var pan_scales := [
		preset["start_scale"] + Vector2(0.03, 0.03),
		preset["start_scale"] + Vector2(0.07, 0.07),
		preset["end_scale"] + Vector2(-0.01, -0.01),
		preset["end_scale"] + Vector2(0.02, 0.02),
		preset["end_scale"],
	]
	var pan_segment_durations := [0.62, 0.58, 0.64, 0.58, 0.58]
	for idx: int in range(pan_offsets.size()):
		var segment_duration: float = pan_segment_durations[idx]
		active_pan_tween.set_parallel(true)
		active_pan_tween.tween_property(scene_image_texture, "position", pan_offsets[idx], segment_duration)
		active_pan_tween.tween_property(scene_image_texture, "scale", pan_scales[idx], segment_duration)
		active_pan_tween.tween_property(scene_image_area, "modulate", Color(1.0, 0.97 - 0.02 * idx, 1.0, 1.0), segment_duration)
		active_pan_tween.set_parallel(false)
	active_pan_tween.finished.connect(_finish_pan)


func _resolve_pan_preset(preset_name: String) -> Dictionary:
	if not preset_name.is_empty():
		var normalized_name := preset_name.strip_edges().to_lower().replace(" ", "_")
		for preset: Dictionary in PAN_PRESETS:
			if str(preset.get("name", "")) == normalized_name:
				return preset

	return PAN_PRESETS[current_step_index % PAN_PRESETS.size()]


func _finish_pan() -> void:
	if not _is_runtime_active():
		return

	scene_image_area.modulate = Color(1, 1, 1, 1)
	is_pan_active = false
	narrative_progression_locked = false
	active_pan_tween = null
	_refresh_continue_prompt()


func _reset_pan_state() -> void:
	if active_pan_tween != null and is_instance_valid(active_pan_tween):
		active_pan_tween.kill()
	active_pan_tween = null
	is_pan_active = false
	scene_image_texture.scale = Vector2.ONE
	scene_image_texture.position = Vector2.ZERO
	scene_image_texture.pivot_offset = scene_image_area.size * 0.5
	scene_image_area.modulate = Color(1, 1, 1, 1)


func _begin_turn_sequence() -> void:
	if not _is_runtime_active():
		return

	narrative_progression_locked = true
	turn_phase_index = 0
	turn_clicks_remaining = TURN_CLICK_COUNT
	turn_resume_ready = false
	_refresh_metric_panel()
	_refresh_continue_prompt()


func _is_turn_consuming_input() -> bool:
	return turn_clicks_remaining > 0


func _advance_turn_sequence() -> void:
	if not _is_runtime_active() or turn_clicks_remaining <= 0:
		return

	turn_phase_index += 1
	turn_clicks_remaining -= 1
	_refresh_metric_panel()

	if turn_clicks_remaining <= 0:
		narrative_progression_locked = false
		turn_resume_ready = true
	_refresh_continue_prompt()


func _clear_turn_visuals_if_pending() -> void:
	if not turn_resume_ready:
		return

	_reset_turn_state(true)
	_refresh_metric_panel()


func _reset_turn_state(clear_visuals: bool) -> void:
	turn_phase_index = 0
	turn_clicks_remaining = 0
	turn_resume_ready = false
	if clear_visuals:
		_clear_turn_visuals()


func _clear_turn_visuals() -> void:
	if not _has_ui_targets():
		return

	scene_image_texture.modulate = Color(1, 1, 1, 1)
	scene_image_area.modulate = Color(1, 1, 1, 1)
	scene_image_texture.position = Vector2.ZERO
	_clear_turn_interference()
	if not is_fading_in:
		fade_overlay.hide()
		fade_overlay.color = Color(0, 0, 0, 0)


func _clear_turn_interference() -> void:
	turn_interference_a.visible = false
	turn_interference_b.visible = false
	turn_interference_a.color = Color(0.88, 0.58, 1.0, 0.0)
	turn_interference_b.color = Color(0.56, 0.78, 1.0, 0.0)


func _apply_turn_visuals() -> void:
	if not _has_ui_targets() or turn_phase_index <= 0:
		return

	var phase_strength := float(turn_phase_index) / float(TURN_CLICK_COUNT)
	var rewrite_flash := 0.55 + 0.45 * absf(sin(metrics_pulse_time * (11.0 + phase_strength * 6.0)))
	var band_pulse := 0.5 + 0.5 * sin(metrics_pulse_time * (8.4 + phase_strength * 4.6))
	var strip_alpha := 0.18 + phase_strength * 0.34
	var glitch_shift: float = floor(sin(metrics_pulse_time * (14.0 + phase_strength * 4.0)) * TURN_GLITCH_SHIFT * phase_strength)
	var vertical_jitter := sin(metrics_pulse_time * 9.0) * 2.0 * phase_strength

	scene_image_texture.position = Vector2(glitch_shift, vertical_jitter)
	scene_image_texture.modulate = Color(
		lerpf(1.0, 0.74, phase_strength * 0.34),
		lerpf(1.0, 0.82, phase_strength * 0.18),
		lerpf(1.0, 1.18, phase_strength * 0.24),
		1.0
	)
	scene_image_area.modulate = Color(
		lerpf(1.0, 0.90, phase_strength * 0.24),
		lerpf(1.0, 0.88, phase_strength * 0.10),
		lerpf(1.0, 1.12, phase_strength * 0.20),
		1.0
	)

	var viewport_height := maxf(1.0, scene_image_area.size.y)
	var sweep_a := fposmod(metrics_pulse_time * (170.0 + 120.0 * phase_strength), maxf(1.0, viewport_height + 180.0)) - 90.0
	var sweep_b := fposmod(metrics_pulse_time * (230.0 + 140.0 * phase_strength) + viewport_height * 0.35, maxf(1.0, viewport_height + 220.0)) - 110.0
	turn_interference_a.visible = true
	turn_interference_b.visible = true
	turn_interference_a.position.y = sweep_a
	turn_interference_b.position.y = sweep_b
	turn_interference_a.size.y = 28.0 + 28.0 * band_pulse
	turn_interference_b.size.y = 18.0 + 24.0 * (1.0 - band_pulse)
	turn_interference_a.color = Color(0.92, 0.58 + 0.10 * band_pulse, 1.0, strip_alpha + 0.16 * rewrite_flash)
	turn_interference_b.color = Color(0.48, 0.82, 1.0, strip_alpha * 0.72 + 0.10 * (1.0 - rewrite_flash))



func _finish_initial_layout() -> void:
	_refresh_frame_layout()
	_show_initial_step()
	call_deferred("_debug_log_layout_state")
	_fade_in_from_black()


func _fade_in_from_black() -> void:
	active_fade_tween = create_tween()
	active_fade_tween.tween_property(fade_overlay, "color", Color(0, 0, 0, 0), FADE_DURATION)
	await active_fade_tween.finished
	if not _has_ui_targets() or is_transitioning:
		active_fade_tween = null
		return

	active_fade_tween = null
	is_fading_in = false
	fade_overlay.hide()
	_refresh_continue_prompt()


func _debug_log_layout_state() -> void:
	print("[GameplayScreen] _ready scene_image_area.size=", scene_image_area.size)
	print("[GameplayScreen] _ready scene_image_area.global_rect=", scene_image_area.get_global_rect())
	print("[GameplayScreen] _ready scene_image_texture.size=", scene_image_texture.size)
	print("[GameplayScreen] _ready scene_image_texture.global_rect=", scene_image_texture.get_global_rect())
	print("[GameplayScreen] _ready scene_frame.global_rect=", scene_frame.get_global_rect())
	print("[GameplayScreen] _ready frame_canvas.global_rect=", frame_canvas.get_global_rect())


func _debug_log_step_advance() -> void:
	print("[GameplayScreen] _advance click current_step_index=", current_step_index, " total_steps=", authored_steps.size())


func _handle_goto_command(target_name: String) -> void:
	if not _is_runtime_active():
		return

	narrative_progression_locked = true

	match _normalize_goto_target(target_name):
		"SURGERY_MODE", "SURGERY_1":
			_enter_surgery_mode()
		"SNAPSHOT", "SNAPSHOT_MODE":
			_enter_snapshot_mode()
		"FINAL":
			_enter_final_screen()
		"F2_OUTCOME_RESOLVE":
			_jump_to_resolved_label(_resolve_outcome_label(_get_current_outcome_id()))
		"F3_OUTCOME_RESOLVE":
			_jump_to_resolved_label(_resolve_outcome_label(_get_current_outcome_id()))
		"F3_EVENT_RESOLVE":
			_jump_to_resolved_label(_resolve_f3_event_label())
		"F2_VIK_LINE_RESOLVE":
			_jump_to_resolved_label(_resolve_f2_victoria_line_label())
		"RESCUE_SEQUENCE":
			_continue_to_next_phase()
		"ANALYTICS_MODE":
			_enter_analytics_mode()
		_:
			if jump_to_label(target_name):
				return
			narrative_progression_locked = false
			push_warning("GameplayScreen: unknown goto target '%s'" % target_name)


func _normalize_goto_target(target_name: String) -> String:
	return target_name.strip_edges().to_upper().replace(" ", "_")


func _normalize_authored_label(label_name: String) -> String:
	return label_name.strip_edges().to_upper().replace(" ", "_")


func _enter_surgery_mode() -> void:
	_log_phase_transition("enter_surgery", current_phase)
	_reset_event_fx_state()
	_begin_scene_transition(SURGERY_SCENE_PATH)


func _enter_snapshot_mode() -> void:
	if _has_game_state():
		GameState.set_gameplay_resume(current_phase, current_step_index)
		GameState.set_snapshot_context(current_phase, false)
	_reset_event_fx_state()
	_begin_scene_transition(SNAPSHOT_SCENE_PATH)


func _enter_final_screen() -> void:
	if _has_game_state() and GameState.ending_id.is_empty():
		GameState.resolve_run_from_allocation()
	_reset_event_fx_state()
	_begin_scene_transition(FINAL_SCENE_PATH)


func _enter_analytics_mode() -> void:
	match current_phase:
		"F3":
			_enter_final_screen()
		_:
			_enter_final_screen()


func _jump_to_resolved_label(label_name: String) -> void:
	if jump_to_label(label_name):
		narrative_progression_locked = false
		return

	narrative_progression_locked = false
	push_warning("GameplayScreen: unresolved authored label '%s' for phase '%s'" % [label_name, current_phase])


func _resolve_f3_event_label() -> String:
	if current_phase != "F3" or not _has_game_state():
		return "F3_POST_OUTCOME"

	var burn: int = int(GameState.film_pressure)
	var oblivion: int = int(GameState.film_oblivion)
	var resolved_label := "F3_POST_OUTCOME"

	if burn > oblivion:
		resolved_label = "F3_EVENT_BURN"
	elif oblivion > burn:
		resolved_label = "F3_EVENT_OBLIVION"

	print("[GameplayScreen] F3_EVENT_RESOLVE pressure=", GameState.film_pressure, " oblivion=", GameState.film_oblivion, " resolved=", resolved_label)
	return resolved_label


func _get_current_outcome_id() -> String:
	if not _has_game_state():
		return ""
	return str(GameState.current_outcome_id).strip_edges().to_upper()


func _resolve_f2_victoria_line_label() -> String:
	var outcome_id := _get_current_outcome_id()
	return str(F2_VICTORIA_LINE_LABELS.get(outcome_id, F2_VICTORIA_LINE_LABELS["12B"]))


func _continue_to_next_phase() -> void:
	var active_phase := current_phase.strip_edges()
	var next_phase := ""
	if _has_game_state():
		next_phase = str(GameState.current_phase).strip_edges()

	if next_phase == active_phase and _has_game_state():
		var advanced_phase := str(GameState.get_next_phase(active_phase)).strip_edges()
		if not advanced_phase.is_empty():
			print("[GameplayScreen] continue_to_next_phase stale next_phase=", next_phase, " active_phase=", active_phase, " fallback=", advanced_phase)
			next_phase = advanced_phase

	if next_phase.is_empty():
		next_phase = GameState.get_next_phase(current_phase) if _has_game_state() else ""

	if next_phase == active_phase and _has_game_state():
		next_phase = str(GameState.get_next_phase(active_phase)).strip_edges()

	if next_phase.is_empty() or next_phase == active_phase:
		_log_phase_transition("continue_to_next_phase.final", active_phase)
		_enter_final_screen()
		return

	if _has_game_state():
		GameState.set_gameplay_resume(next_phase, 0)
		GameState.clear_snapshot_context()
	_log_phase_transition("continue_to_next_phase.advance", active_phase, next_phase)
	_begin_scene_transition(GAMEPLAY_SCENE_PATH)


func _set_phase(phase_id: String) -> void:
	if not PHASE_FLOW.has(phase_id):
		push_warning("GameplayScreen: unknown phase id '%s'" % phase_id)
		return

	current_phase = phase_id
	if _has_game_state():
		GameState.current_phase = phase_id
	_log_phase_transition("set_phase", current_phase)
	_refresh_phase_headers()
	_reset_event_fx_state()
	_apply_default_image_for_phase()


func _log_phase_transition(reason: String, phase_override: String = "", next_phase: String = "", surgery_result: String = "") -> void:
	var phase_value := phase_override if not phase_override.is_empty() else current_phase
	print(
		"[GameplayScreen] phase_transition reason=", reason,
		" current_phase=", phase_value,
		" current_step=", current_step_index,
		" next_phase=", next_phase if not next_phase.is_empty() else "-",
		" surgery_result=", surgery_result if not surgery_result.is_empty() else "-"
	)


func play_oblivion_event_fx() -> void:
	_play_event_fx(EVENT_FX_OBLIVION)


func play_burn_event_fx() -> void:
	_play_event_fx(EVENT_FX_BURN)


func clear_event_fx() -> void:
	_stop_event_fx_tween()
	active_event_fx = EVENT_FX_NONE
	var oblivion_amount := _get_event_overlay_amount(oblivion_event_overlay)
	var burn_amount := _get_event_overlay_amount(burn_event_overlay)
	var should_fade := oblivion_amount > 0.001 or burn_amount > 0.001
	if not should_fade:
		_reset_event_fx_state()
		return

	active_event_fx_tween = create_tween()
	active_event_fx_tween.set_parallel(true)
	active_event_fx_tween.set_trans(Tween.TRANS_SINE)
	active_event_fx_tween.set_ease(Tween.EASE_OUT)
	active_event_fx_tween.tween_method(_set_event_overlay_amount.bind(oblivion_event_overlay), oblivion_amount, 0.0, EVENT_FX_FADE_OUT_DURATION)
	active_event_fx_tween.tween_method(_set_event_overlay_amount.bind(burn_event_overlay), burn_amount, 0.0, EVENT_FX_FADE_OUT_DURATION)
	active_event_fx_tween.finished.connect(_reset_event_fx_state)


func _handle_event_fx_label(label_name: String) -> void:
	match label_name:
		"F3_EVENT_OBLIVION":
			play_oblivion_event_fx()
		"F3_EVENT_BURN":
			play_burn_event_fx()
		_:
			clear_event_fx()


func _play_event_fx(effect_name: String) -> void:
	if not _has_ui_targets():
		return

	_sync_event_overlay_textures()
	_stop_event_fx_tween()
	active_event_fx = effect_name

	var target_overlay := _get_event_overlay_node(effect_name)
	var other_overlay := burn_event_overlay if target_overlay == oblivion_event_overlay else oblivion_event_overlay
	if target_overlay == null:
		_reset_event_fx_state()
		return

	if is_instance_valid(other_overlay):
		other_overlay.visible = false
		_set_event_overlay_amount(0.0, other_overlay)
	target_overlay.visible = true
	target_overlay.texture = scene_image_texture.texture

	var start_amount := _get_event_overlay_amount(target_overlay)
	active_event_fx_tween = create_tween()
	active_event_fx_tween.set_trans(Tween.TRANS_SINE)
	active_event_fx_tween.set_ease(Tween.EASE_OUT)
	active_event_fx_tween.tween_method(_set_event_overlay_amount.bind(target_overlay), start_amount, 1.0, EVENT_FX_FADE_IN_DURATION)


func _get_event_overlay_node(effect_name: String) -> TextureRect:
	match effect_name:
		EVENT_FX_OBLIVION:
			return oblivion_event_overlay
		EVENT_FX_BURN:
			return burn_event_overlay
		_:
			return null


func _sync_event_overlay_textures() -> void:
	if not is_instance_valid(scene_image_texture):
		return

	var base_texture := scene_image_texture.texture
	if is_instance_valid(oblivion_event_overlay):
		oblivion_event_overlay.texture = base_texture
	if is_instance_valid(burn_event_overlay):
		burn_event_overlay.texture = base_texture


func _get_event_overlay_amount(overlay: TextureRect) -> float:
	if not is_instance_valid(overlay):
		return 0.0
	var shader_material := overlay.material as ShaderMaterial
	if shader_material == null:
		return 0.0
	return float(shader_material.get_shader_parameter("amount"))


func _set_event_overlay_amount(value: float, overlay: TextureRect) -> void:
	if not is_instance_valid(overlay):
		return
	var shader_material := overlay.material as ShaderMaterial
	if shader_material == null:
		return
	shader_material.set_shader_parameter("amount", clampf(value, 0.0, 1.0))


func _reset_event_fx_state() -> void:
	_stop_event_fx_tween()
	active_event_fx = EVENT_FX_NONE
	if is_instance_valid(oblivion_event_overlay):
		oblivion_event_overlay.visible = false
		_set_event_overlay_amount(0.0, oblivion_event_overlay)
	if is_instance_valid(burn_event_overlay):
		burn_event_overlay.visible = false
		_set_event_overlay_amount(0.0, burn_event_overlay)


func _stop_event_fx_tween() -> void:
	if is_instance_valid(active_event_fx_tween):
		active_event_fx_tween.kill()
	active_event_fx_tween = null


func _should_show_snapshot_after_branch() -> bool:
	if not _has_game_state():
		return false
	var snapshot_context: Dictionary = GameState.get_snapshot_context()
	return not str(snapshot_context.get("source_phase", "")).strip_edges().is_empty()


func _return_to_title() -> void:
	if _has_game_state():
		GameState.reset_run()
	_begin_scene_transition(TITLE_SCENE_PATH)


func _build_completed_summary() -> String:
	if not _has_game_state() or GameState.completed_phases.is_empty():
		return "none"
	return ", ".join(GameState.completed_phases)


func _build_allocation_summary() -> String:
	if not _has_game_state():
		return "Scene 0 / Victoria 0 / Desmond 0"
	return "Scene %d / Victoria %d / Desmond %d" % [
		int(GameState.surgery_allocation.get("scene", 0)),
		int(GameState.surgery_allocation.get("victoria", 0)),
		int(GameState.surgery_allocation.get("desmond", 0)),
	]


func _refresh_metric_panel() -> void:
	if not _can_run_runtime_updates() or not _has_ui_targets():
		return

	if not _has_game_state():
		dossier_slot_a_headshot_label.text = "ФИЛЬМ"
		dossier_slot_a_title.text = "Глубина 0"
		dossier_slot_a_cue.visible = true
		dossier_slot_a_cue.text = "Забвение 0 [низкое]\nДавление 0 [низкое]"
		dossier_slot_b_headshot_label.text = "ГЕРОИ"
		var fallback_cast_lines := _build_hud_cast_metric_lines(true)
		dossier_slot_b_title.text = "\n".join(fallback_cast_lines)
		dossier_slot_b_cue.visible = true
		dossier_slot_b_cue.text = HUD_CAST_LEGEND_COPY
		dossier_slot_c_headshot_label.text = "АКЦЕНТ"
		dossier_slot_c_title.text = "0"
		dossier_slot_c_cue.visible = false
		dossier_slot_c_cue.text = ""
		_apply_metric_panel_corruption()
		return

	dossier_slot_a_headshot_label.text = "ФИЛЬМ"
	dossier_slot_a_title.text = "Глубина %d" % GameState.film_depth
	dossier_slot_a_cue.visible = true
	dossier_slot_a_cue.text = "Забвение %d [%s]\nДавление %d [%s]" % [
		GameState.film_oblivion,
		_risk_tag(int(GameState.film_oblivion)),
		GameState.film_pressure,
		_risk_tag(int(GameState.film_pressure)),
	]
	dossier_slot_b_headshot_label.text = "ГЕРОИ"
	var cast_lines := _build_hud_cast_metric_lines(true)
	dossier_slot_b_title.text = "\n".join(cast_lines)
	dossier_slot_b_cue.visible = true
	dossier_slot_b_cue.text = HUD_CAST_LEGEND_COPY
	dossier_slot_c_headshot_label.text = "АКЦЕНТ"
	dossier_slot_c_title.text = "%d" % GameState.control_next
	dossier_slot_c_cue.visible = false
	dossier_slot_c_cue.text = ""
	_apply_metric_panel_corruption()


func _apply_metric_panel_corruption() -> void:
	if not _can_run_runtime_updates() or not _has_ui_targets():
		return

	var oblivion_ratio: float = 0.0
	var pressure_ratio: float = 0.0
	var oblivion_value: int = 0
	var pressure_value: int = 0
	if _has_game_state():
		oblivion_value = int(GameState.film_oblivion)
		pressure_value = int(GameState.film_pressure)
		oblivion_ratio = clampf(float(oblivion_value) / FILM_METRIC_MAX, 0.0, 1.0)
		pressure_ratio = clampf(float(pressure_value) / FILM_METRIC_MAX, 0.0, 1.0)

	var oblivion_band: int = _risk_band(oblivion_value)
	var pressure_band: int = _risk_band(pressure_value)
	var global_band: int = maxi(oblivion_band, pressure_band)

	var base_style := StyleBoxFlat.new()
	base_style.bg_color = Color(
		lerpf(0.08, 0.26, oblivion_ratio * 0.50),
		lerpf(0.07, 0.24, oblivion_ratio * 0.54),
		lerpf(0.06, 0.29, max(oblivion_ratio * 0.62, pressure_ratio * 0.16)),
		lerpf(0.94, 0.80, oblivion_ratio)
	)
	base_style.border_width_left = 2
	base_style.border_width_top = 2
	base_style.border_width_right = 2
	base_style.border_width_bottom = 2
	base_style.border_color = Color(
		lerpf(0.39, 0.72, pressure_ratio * 0.90),
		lerpf(0.32, 0.38, oblivion_ratio * 0.55),
		lerpf(0.25, 0.96, pressure_ratio * 0.95),
		lerpf(0.85, 0.98, max(oblivion_ratio * 0.8, pressure_ratio))
	)
	base_style.corner_radius_top_left = 8
	base_style.corner_radius_top_right = 8
	base_style.corner_radius_bottom_right = 12
	base_style.corner_radius_bottom_left = 12
	base_style.shadow_color = Color(0.33, 0.10, 0.55, 0.10 + pressure_ratio * 0.30)
	base_style.shadow_size = 8 + int(round(pressure_ratio * 12.0))
	portrait_strip.add_theme_stylebox_override("panel", base_style)

	var film_card_band: int = maxi(oblivion_band, pressure_band)
	var heroes_card_band: int = max(oblivion_band - 1, pressure_band)
	var accent_card_band: int = maxi(1, maxi(oblivion_band, pressure_band))
	var card_bands: Array[int] = [film_card_band, heroes_card_band, accent_card_band]
	var card_index: int = 0
	for card: PanelContainer in [dossier_slot_a, dossier_slot_b, dossier_slot_c]:
		var card_band: int = card_bands[card_index]
		var card_heat: float = [0.0, 0.42, 0.82][card_band]
		var card_fade: float = [0.0, 0.34, 0.68][card_band]
		var card_style := StyleBoxFlat.new()
		card_style.bg_color = Color(
			lerpf(0.18, 0.34, max(oblivion_ratio * 0.30, card_heat * 0.12)),
			lerpf(0.18, 0.24, oblivion_ratio * 0.24),
			lerpf(0.16, 0.33, max(oblivion_ratio * 0.34, pressure_ratio * 0.30)),
			lerpf(1.0, 0.84, max(oblivion_ratio * 0.7, card_fade * 0.5))
		)
		card_style.border_width_left = 1
		card_style.border_width_top = 1
		card_style.border_width_right = 1
		card_style.border_width_bottom = 1
		card_style.border_color = Color(
			lerpf(0.51, 0.86, pressure_ratio * 0.78 + card_heat * 0.12),
			lerpf(0.45, 0.40, oblivion_ratio * 0.55),
			lerpf(0.35, 0.98, pressure_ratio * 0.88 + card_heat * 0.08),
			lerpf(0.80, 0.96, max(pressure_ratio * 0.9, oblivion_ratio * 0.65, float(card_band) * 0.22))
		)
		card_style.corner_radius_top_left = 6
		card_style.corner_radius_top_right = 6
		card_style.corner_radius_bottom_right = 6
		card_style.corner_radius_bottom_left = 6
		card.add_theme_stylebox_override("panel", card_style)
		card_index += 1

	var heading_alpha: float = lerpf(0.92, 0.54, oblivion_ratio)
	var title_alpha: float = lerpf(0.98, 0.70, oblivion_ratio * 0.70)
	var cue_alpha: float = lerpf(0.88, 0.42, oblivion_ratio)
	for heading: Label in [dossier_slot_a_headshot_label, dossier_slot_b_headshot_label, dossier_slot_c_headshot_label]:
		heading.modulate = Color(
			lerpf(0.88, 1.00, pressure_ratio * 0.26),
			lerpf(0.90, 0.84, oblivion_ratio * 0.26),
			lerpf(0.95, 1.00, pressure_ratio * 0.34),
			heading_alpha
		)
	var title_colors: Array[Color] = [
		Color(
			lerpf(0.92, 1.00, pressure_ratio * 0.18),
			lerpf(0.92, 0.82, oblivion_ratio * 0.26),
			lerpf(0.94, 1.00, pressure_ratio * 0.26),
			title_alpha
		),
		Color(
			lerpf(0.92, 0.96, float(global_band) * 0.12),
			lerpf(0.92, 0.85, oblivion_ratio * 0.20),
			lerpf(0.94, 0.99, pressure_ratio * 0.16),
			title_alpha
		),
		Color(
			lerpf(0.92, 0.95, float(global_band) * 0.08),
			lerpf(0.92, 0.84, oblivion_ratio * 0.24),
			lerpf(0.94, 0.97, pressure_ratio * 0.12),
			lerpf(title_alpha, cue_alpha, 0.2)
		),
	]
	var title_index: int = 0
	for title: Label in [dossier_slot_a_title, dossier_slot_b_title, dossier_slot_c_title]:
		title.modulate = title_colors[title_index]
		title_index += 1
	var cue_colors: Array[Color] = [
		Color(
			lerpf(0.84, 0.96, pressure_ratio * 0.24),
			lerpf(0.76, 0.74, pressure_ratio * 0.10),
			lerpf(0.70, 0.98, pressure_ratio * 0.42),
			cue_alpha
		),
		Color(
			lerpf(0.82, 0.90, pressure_ratio * 0.14),
			lerpf(0.74, 0.72, oblivion_ratio * 0.06),
			lerpf(0.68, 0.92, pressure_ratio * 0.24),
			lerpf(cue_alpha, cue_alpha * 0.92, float(global_band) * 0.15)
		),
		Color(
			lerpf(0.80, 0.88, pressure_ratio * 0.10),
			lerpf(0.74, 0.70, oblivion_ratio * 0.10),
			lerpf(0.68, 0.86, pressure_ratio * 0.16),
			lerpf(cue_alpha, cue_alpha * 0.84, oblivion_ratio * 0.24)
		),
	]
	var cue_index: int = 0
	for cue: Label in [dossier_slot_a_cue, dossier_slot_b_cue, dossier_slot_c_cue]:
		cue.modulate = cue_colors[cue_index]
		cue_index += 1

	dossier_slot_c_headshot_label.modulate = Color(
		lerpf(0.96, 1.0, pressure_ratio * 0.22),
		lerpf(0.88, 0.82, oblivion_ratio * 0.16),
		lerpf(0.82, 1.0, pressure_ratio * 0.32),
		lerpf(0.94, 1.0, float(global_band) * 0.12)
	)
	dossier_slot_c_title.modulate = Color(
		lerpf(1.0, 1.0, pressure_ratio),
		lerpf(0.94, 0.86, oblivion_ratio * 0.18),
		lerpf(0.88, 1.0, pressure_ratio * 0.28),
		lerpf(0.98, 1.0, float(global_band) * 0.08)
	)

	var oblivion_flicker: float = 0.55 + 0.45 * absf(sin(metrics_pulse_time * (3.1 + oblivion_ratio * 2.8)))
	var pressure_shimmer: float = 0.72 + 0.28 * sin(metrics_pulse_time * (2.0 + pressure_ratio * 2.2))
	var danger_flash: float = 0.55 + 0.45 * absf(sin(metrics_pulse_time * (4.8 + float(global_band) * 0.7)))
	portrait_bleach_overlay.color = Color(0.92, 0.94, 0.95, 0.08 + oblivion_ratio * 0.34)
	portrait_oblivion_static_a.color = Color(0.94, 0.96, 0.98, oblivion_ratio * (0.08 + 0.14 * oblivion_flicker))
	portrait_oblivion_static_b.color = Color(0.80, 0.84, 0.88, oblivion_ratio * (0.06 + 0.12 * (1.0 - oblivion_flicker)))
	portrait_oblivion_grime_left.color = Color(0.60, 0.64, 0.68, oblivion_ratio * (0.10 + 0.14 * oblivion_flicker))
	portrait_oblivion_grime_right.color = Color(0.60, 0.64, 0.68, oblivion_ratio * (0.11 + 0.13 * (1.0 - oblivion_flicker)))

	var edge_alpha: float = pressure_ratio * (0.10 + 0.24 * pressure_shimmer)
	var fracture_alpha: float = pressure_ratio * (0.08 + 0.16 * absf(sin(metrics_pulse_time * 5.8)))
	var burn_flash_alpha: float = pressure_ratio * float(pressure_band) * 0.04 * danger_flash
	portrait_pressure_edge_top.color = Color(0.61, 0.30, 0.92, edge_alpha + burn_flash_alpha)
	portrait_pressure_edge_bottom.color = Color(0.74, 0.36, 1.00, edge_alpha * 1.18 + burn_flash_alpha)
	portrait_pressure_fracture_left.color = Color(0.70, 0.34, 1.00, fracture_alpha)
	portrait_pressure_fracture_right.color = Color(0.78, 0.38, 1.00, fracture_alpha * 1.08)

	if turn_phase_index > 0:
		_apply_turn_visuals()


func _begin_scene_transition(target_scene_path: String) -> void:
	if is_transitioning:
		return

	is_transitioning = true
	narrative_progression_locked = true
	_stop_runtime_activity()
	call_deferred("_change_scene_deferred", target_scene_path)


func _change_scene_deferred(target_scene_path: String) -> void:
	var tree := get_tree()
	if tree == null:
		return

	tree.change_scene_to_file(target_scene_path)


func _stop_runtime_activity() -> void:
	set_process(false)
	set_process_input(false)
	set_physics_process(false)
	_reset_pan_state()
	_reset_turn_state(true)
	_hide_continue_prompt()
	_kill_active_tweens()
	pending_octavia_clear_clicks = -1
	is_pan_active = false


func _kill_active_tweens() -> void:
	if active_fade_tween != null and is_instance_valid(active_fade_tween):
		active_fade_tween.kill()
	active_fade_tween = null

	if active_pan_tween != null and is_instance_valid(active_pan_tween):
		active_pan_tween.kill()
	active_pan_tween = null


func _is_runtime_active() -> bool:
	return is_instance_valid(self) and is_inside_tree() and not is_transitioning


func _can_run_runtime_updates() -> bool:
	return _is_runtime_active()


func _is_input_available() -> bool:
	return _is_runtime_active()


func _has_ui_targets() -> bool:
	return is_instance_valid(self) and is_inside_tree() and is_instance_valid(scene_image_texture) and is_instance_valid(portrait_strip) and is_instance_valid(fade_overlay)


func _has_game_state() -> bool:
	return is_instance_valid(self) and is_inside_tree() and is_instance_valid(GameState)


func _mark_input_handled() -> void:
	var viewport := get_viewport()
	if viewport != null:
		viewport.set_input_as_handled()


func _risk_band(value: int) -> int:
	if value >= 7:
		return 2
	if value >= 4:
		return 1
	return 0


func _risk_tag(value: int) -> String:
	match _risk_band(value):
		2:
			return "высокое"
		1:
			return "среднее"
		_:
			return "низкое"


func _risk_word(value: int) -> String:
	if value >= 7:
		return "high"
	if value >= 4:
		return "rising"
	return "low"


func _build_hud_cast_metric_lines(use_short_labels: bool) -> PackedStringArray:
	var lines := PackedStringArray()
	for character_id: String in HUD_CAST_CHARACTER_ORDER:
		lines.append(_format_hud_cast_metric_line(character_id, use_short_labels))
	return lines


func _format_hud_cast_metric_line(character_id: String, use_short_labels: bool) -> String:
	var integrity_value := 0
	var trauma_value := 0
	if _has_game_state():
		integrity_value = int(GameState.get("%s_integrity" % character_id))
		trauma_value = int(GameState.get("%s_trauma" % character_id))
	var character_name := str(HUD_CAST_CHARACTER_SHORT_NAMES.get(character_id, character_id.left(1).to_upper()))
	if use_short_labels:
		return "%s: СБ %d / СЛ %d" % [character_name, integrity_value, trauma_value]
	return "%s: Самобытность %d / След %d" % [character_name, integrity_value, trauma_value]


func _resolve_hud_focus_label(focus_id: String) -> String:
	match focus_id.strip_edges().to_lower():
		"desmond":
			return "Дезмонд"
		"victoria":
			return "Виктория"
		"scene":
			return "Сцена"
		_:
			return "Оставлено как есть"
