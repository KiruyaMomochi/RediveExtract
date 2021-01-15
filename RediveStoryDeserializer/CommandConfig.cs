namespace RediveStoryDeserializer
{
    public record CommandConfig
    {
        public CommandNumber Number { get; init; }
        public string Name { get; init; }
        public string ClassName { get; init; }
        public CommandCategory CommandCategory { get; init; }
        public (int, int) ArgCount { get; init; }

        public static readonly CommandConfig[] List = new[]
        {
            new CommandConfig {
                Number = CommandNumber.Title,
                Name = "title",
                ClassName = "StoryCommandTitle",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig {
                Number = CommandNumber.Outline,
                Name = "outline",
                ClassName = "StoryCommandOutline",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Visible,
                Name = "visible",
                ClassName = "StoryCommandVisible",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Face,
                Name = "face",
                ClassName = "StoryCommandFace",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Focus,
                Name = "focus",
                ClassName = "StoryCommandFocus",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Background,
                Name = "background",
                ClassName = "StoryCommandBackground",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Print,
                Name = "print",
                ClassName = "StoryCommandPrint",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Tag,
                Name = "tag",
                ClassName = "StoryCommandTag",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Goto,
                Name = "goto",
                ClassName = "StoryCommandGoto",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Bgm,
                Name = "bgm",
                ClassName = "StoryCommandBgm",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 6)
            },
            new CommandConfig{
                Number = CommandNumber.Touch,
                Name = "touch",
                ClassName = "StoryCommandTouch",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.Choice,
                Name = "choice",
                ClassName = "StoryCommandChoice",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Vo,
                Name = "vo",
                ClassName = "StoryCommandVo",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Wait,
                Name = "wait",
                ClassName = "StoryCommandWait",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.InL,
                Name = "in_L",
                ClassName = "StoryCommandInL",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 3)
            },
            new CommandConfig{
                Number = CommandNumber.InR,
                Name = "in_R",
                ClassName = "StoryCommandInR",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 3)
            },
            new CommandConfig{
                Number = CommandNumber.OutL,
                Name = "out_L",
                ClassName = "StoryCommandOutL",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 3)
            },
            new CommandConfig{
                Number = CommandNumber.OutR,
                Name = "out_R",
                ClassName = "StoryCommandOutR",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 3)
            },
            new CommandConfig{
                Number = CommandNumber.Fadein,
                Name = "fadein",
                ClassName = "StoryCommandFadein",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Fadeout,
                Name = "fadeout",
                ClassName = "StoryCommandFadeout",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.InFloat,
                Name = "in_float",
                ClassName = "StoryCommandInFloat",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.OutFloat,
                Name = "out_float",
                ClassName = "StoryCommandOutFloat",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Jump,
                Name = "jump",
                ClassName = "StoryCommandJump",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Shake,
                Name = "shake",
                ClassName = "StoryCommandShake",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Pop,
                Name = "pop",
                ClassName = "StoryCommandPop",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Nod,
                Name = "nod",
                ClassName = "StoryCommandNod",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Se,
                Name = "se",
                ClassName = "StoryCommandSe",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.BlackOut,
                Name = "black_out",
                ClassName = "StoryCommandBlackOut",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.BlackIn,
                Name = "black_in",
                ClassName = "StoryCommandBlackIn",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.WhiteOut,
                Name = "white_out",
                ClassName = "StoryCommandWhiteOut",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.WhiteIn,
                Name = "white_in",
                ClassName = "StoryCommandWhiteIn",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Transition,
                Name = "transition",
                ClassName = "StoryCommandTransition",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Situation,
                Name = "situation",
                ClassName = "StoryCommandSituation",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.ColorFadein,
                Name = "color_fadein",
                ClassName = "StoryCommandColorFadein",
                CommandCategory = CommandCategory.System,
                ArgCount = (3, 3)
            },
            new CommandConfig{
                Number = CommandNumber.Flash,
                Name = "flash",
                ClassName = "StoryCommandFlash",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.ShakeText,
                Name = "shake_text",
                ClassName = "StoryCommandShakeText",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.TextSize,
                Name = "text_size",
                ClassName = "StoryCommandTextSize",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.ShakeScreen,
                Name = "shake_screen",
                ClassName = "StoryCommandShakeScreen",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.Double,
                Name = "double",
                ClassName = "StoryCommandDouble",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 4)
            },
            new CommandConfig{
                Number = CommandNumber.Scale,
                Name = "scale",
                ClassName = "StoryCommandScale",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 4)
            },
            new CommandConfig{
                Number = CommandNumber.TitleTelop,
                Name = "title_telop",
                ClassName = "StoryCommandTitleTelop",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.WindowVisible,
                Name = "window_visible",
                ClassName = "StoryCommandWindowVisible",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Log,
                Name = "log",
                ClassName = "StoryCommandLog",
                CommandCategory = CommandCategory.System,
                ArgCount = (3, 4)
            },
            new CommandConfig{
                Number = CommandNumber.NoVoice,
                Name = "novoice",
                ClassName = "StoryCommandNoVoice",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.Change,
                Name = "change",
                ClassName = "StoryCommandChange",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (2, 3)
            },
            new CommandConfig{
                Number = CommandNumber.FadeoutAll,
                Name = "fadeout_all",
                ClassName = "StoryCommandFadeoutAll",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Movie,
                Name = "movie",
                ClassName = "StoryCommandMovie",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 3)
            },
            new CommandConfig{
                Number = CommandNumber.MovieStay,
                Name = "movie_stay",
                ClassName = "StoryCommandMovieStay",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.Battle,
                Name = "battle",
                ClassName = "StoryCommandBattle",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.Still,
                Name = "still",
                ClassName = "StoryCommandStill",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 5)
            },
            new CommandConfig{
                Number = CommandNumber.BustUp,
                Name = "bust",
                ClassName = "StoryCommandBustup",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 3)
            },
            new CommandConfig{
                Number = CommandNumber.Env,
                Name = "amb",
                ClassName = "StoryCommandEnv",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 4)
            },
            new CommandConfig{
                Number = CommandNumber.TutorialReward,
                Name = "reward",
                ClassName = "StoryCommandTutorialReward",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.NameEdit,
                Name = "name_dialog",
                ClassName = "StoryCommandPlayerNameEdit",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.Effect,
                Name = "effect",
                ClassName = "StoryCommandParticleEffect",
                CommandCategory = CommandCategory.Effect,
                ArgCount = (1, 5)
            },
            new CommandConfig{
                Number = CommandNumber.EffectDelete,
                Name = "effect_delete",
                ClassName = "StoryCommandParticleDelete",
                CommandCategory = CommandCategory.Effect,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.EyeOpen,
                Name = "eye_open",
                ClassName = "StoryCommandEyeOpen",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.MouthOpen,
                Name = "mouth_open",
                ClassName = "StoryCommandMouthOpen",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.AutoEnd,
                Name = "end",
                ClassName = "StoryCommandForcedEnd",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.Emotion,
                Name = "emotion",
                ClassName = "StoryCommandEmotion",
                CommandCategory = CommandCategory.Effect,
                ArgCount = (1, 5)
            },
            new CommandConfig{
                Number = CommandNumber.EmotionEnd,
                Name = "emotion_end",
                ClassName = "StoryCommandEmotionEnd",
                CommandCategory = CommandCategory.Effect,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.EnvStop,
                Name = "amb_stop",
                ClassName = "StoryCommandEnvStop",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.BgmPause,
                Name = "bgm_stop",
                ClassName = "StoryCommandBgmPause",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.BgmResume,
                Name = "bgm_resume",
                ClassName = "StoryCommandBgmResume",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.BgmVolumeChange,
                Name = "bgm_volume",
                ClassName = "StoryCommandBgmVolumeChange",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.EnvResume,
                Name = "amb_resume",
                ClassName = "StoryCommandEnvResume",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.EnvVolume,
                Name = "amb_volume",
                ClassName = "StoryCommandEnvVolumeChange",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 2)
            },
            new CommandConfig{
                Number = CommandNumber.SePause,
                Name = "se_pause",
                ClassName = "StoryCommandSeStop",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 2)
            },
            new CommandConfig{
                Number = CommandNumber.CharaFull,
                Name = "chara_full",
                ClassName = "StoryCommandCharacterFull",
                CommandCategory = CommandCategory.System,
                ArgCount = (3, 4)
            },
            new CommandConfig{
                Number = CommandNumber.Sway,
                Name = "sway",
                ClassName = "StoryCommandSway",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.BackgroundColor,
                Name = "bg_color",
                ClassName = "StoryCommandBackgroundColor",
                CommandCategory = CommandCategory.System,
                ArgCount = (3, 4)
            },
            new CommandConfig{
                Number = CommandNumber.Pan,
                Name = "pan",
                ClassName = "StoryCommandStillPan",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.StillUnit,
                Name = "still_unit",
                ClassName = "StoryCommandStillUnit",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.SlideChara,
                Name = "slide",
                ClassName = "StoryCommandSlideCharacter",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.ShakeScreenOnce,
                Name = "shake_once",
                ClassName = "StoryCommandShakeScreenOnce",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.TransitionResume,
                Name = "transition_resume",
                ClassName = "StoryCommandTransitionResume",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.ShakeLoop,
                Name = "shake_loop",
                ClassName = "StoryCommandShakeLoop",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.ShakeDelete,
                Name = "shake_delete",
                ClassName = "StoryCommandShakeDelete",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.UnFace,
                Name = "unface",
                ClassName = "StoryCommandUnface",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.WaitToken,
                Name = "token",
                ClassName = "StoryCommandWaitToken",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.EffectEnv,
                Name = "effect_env",
                ClassName = "StoryCommandParticleEffectEnv",
                CommandCategory = CommandCategory.Effect,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.BrightChange,
                Name = "bright_change",
                ClassName = "StoryCommandBrightChange",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.CharaShadow,
                Name = "chara_shadow",
                ClassName = "StoryCommandCharacterShadow",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.UiVisible,
                Name = "ui_visible",
                ClassName = "StoryCommandMenuVisible",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.FadeinAll,
                Name = "fadein_all",
                ClassName = "StoryCommandFadeinAll",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.ChangeWindow,
                Name = "change_window",
                ClassName = "StoryCommandChangeWindow",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.BgPan,
                Name = "bg_pan",
                ClassName = "StoryCommandBackgroundPan",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 3)
            },
            new CommandConfig{
                Number = CommandNumber.StillMove,
                Name = "still_move",
                ClassName = "StoryCommandStillMove",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 3)
            },
            new CommandConfig{
                Number = CommandNumber.StillNormalize,
                Name = "still_normalize",
                ClassName = "StoryCommandStillNormalize",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.VoiceEffect,
                Name = "vo_effect",
                ClassName = "StoryCommandVoiceEffect",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.TrialEnd,
                Name = "trial_end",
                ClassName = "StoryCommandTrialEnd",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.SeEffect,
                Name = "se_effect",
                ClassName = "StoryCommandSeEffect",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.CharacterUpDown,
                Name = "updown",
                ClassName = "StoryCommandCharacterUpdown",
                CommandCategory = CommandCategory.Motion,
                ArgCount = (4, 4)
            },
            new CommandConfig{
                Number = CommandNumber.BgCameraZoom,
                Name = "bg_zoom",
                ClassName = "StoryCommandBgCameraZoom",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.BackgroundSplit,
                Name = "split",
                ClassName = "StoryCommandBackgroundSplit",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 6)
            },
            new CommandConfig{
                Number = CommandNumber.CameraZoom,
                Name = "camera_zoom",
                ClassName = "StoryCommandCameraZoom",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.SplitSlide,
                Name = "split_slide",
                ClassName = "StoryCommandBackgroundSplitSlide",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 6)
            },
            new CommandConfig{
                Number = CommandNumber.BgmTransition,
                Name = "bgm_transition",
                ClassName = "StoryCommandBgmTransition",
                CommandCategory = CommandCategory.System,
                ArgCount = (2, 2)
            },
            new CommandConfig{
                Number = CommandNumber.ShakeAnime,
                Name = "shake_anime",
                ClassName = "StoryCommandBackgroundAnimation",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.InsertStory,
                Name = "insert",
                ClassName = "StoryCommandInsertStory",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig{
                Number = CommandNumber.Place,
                Name = "place",
                ClassName = "StoryCommandPlace",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.IgnoreBgm,
                Name = "bgm_overview",
                ClassName = "StoryCommandBgmIgnoreStop",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 5)
            },
            new CommandConfig{
                Number = CommandNumber.MultiLipsync,
                Name = "multi_talk",
                ClassName = "StoryCommandMultiLipsync",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 1)
            },
            new CommandConfig{
                Number = CommandNumber.Jingle,
                Name = "jingle_start",
                ClassName = "StoryCommandJingle",
                CommandCategory = CommandCategory.System,
                ArgCount = (1, 1)
            },
            new CommandConfig{
                Number = CommandNumber.TouchToStart,
                Name = "touch_to_start",
                ClassName = "StoryCommandTouchToStart",
                CommandCategory = CommandCategory.System,
                ArgCount = (0, 0)
            },
            new CommandConfig
            {
                Number = CommandNumber.EventAdvMoveHorizontal
            },
            new CommandConfig
            {
                Number = CommandNumber.BgPanX,
                Name = "background_pan_x",
                ClassName = "StoryCommandBackgroundPanX"
            },
            new CommandConfig()
            {
                Number = CommandNumber.BackgroundBlur,
                Name = "background_blur",
                ClassName = "StoryCommandBackgroundBlur"
            },
            new CommandConfig()
            {
                Number = CommandNumber.SeasonalReward,
                Name = "seasonal_reward",
                ClassName = "StoryCommandSeasonalReward"
            },
            new CommandConfig()
            {
                Number = CommandNumber.MiniGame,
                Name = "mini_game",
                ClassName = "StoryCommandMiniGame"
            },
            new CommandConfig()
            {
                Number = CommandNumber.DialogAnimation,
                Name = "dialog_animation",
                ClassName = "StoryCommandDialogAnimation"
            }
        };
    }
}
