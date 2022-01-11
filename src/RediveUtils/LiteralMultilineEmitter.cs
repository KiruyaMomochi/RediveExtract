using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace RediveUtils;

public class LiteralMultilineEmitter : ChainedEventEmitter
{
    public LiteralMultilineEmitter(IEventEmitter nextEmitter) : base(nextEmitter)
    {
    }

    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        if (eventInfo.Source.Value is string str)
        {
            if (str.Contains('\n') && !str.Contains(" \n") && !str.EndsWith(" "))
                eventInfo.Style = ScalarStyle.Literal;
        }

        base.Emit(eventInfo, emitter);
    }
}
