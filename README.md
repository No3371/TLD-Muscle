# Muscle

Eat well, move a lot, and properly sleep, then you will gain muscle.

The gained strength will not make you a superman, but silently buff you in various aspects and make your life easier.

The mod gives a purpose for players to stay fed, take care of various needs, avoid injuries and sickness and excercise like to run when it's unnecessary.

The mod doesn't do much and could even be harmful for players whose playstyle involves intense hibernation.

## Basics

Muscle **mildly** affects these player abilities:

- Carry capacity (Not mildly actually, goes up to 5kg but it takes a lot of time)
- Fatigue increments
- Tiredness thresholds
- Climbing fatigue
- Movement in bad weathers
- Struggling

To maximize muscle gains, you will want to:

- Stay fed. It would requires quite some foods for muscle gains, therefore Hunting is essential
- Move. Burn your calories
- Avoid segmented sleep, try to sleep up to 10 hours in one go

To avoid or minimize muscle loss, you will want to:

- Avoid sleep deprivation
- Avoid long starvation between meals
- Avoid hibernation
- Avoid sickness
- Avoid neglecting low condition

TL;DR: Take care. Be healthy.

## Details

### Muscle Gain

Carry Capacity is an essential element of TLD gameplay, therefore, the mod is designed to be harsher on muscle growth than on muscle reduction.

The muscle gain attempt happens when you wake up. The first requirement is the hours you sleep, sleep shorter than 5 hours simply won't trigger muscle gain.

Then, the hours since last time you wake up from 5+ hours sleep kicks in. The longer is this span, the more calories eaten and burned are required for muscle gain to happens. Combining with the above condition, segmented sleeps will makes muscle gains harder to trigger, because the hours you invested in segmented sleeps are still counted but you are not eat and move as much as being awake and active.

Finally, your condition must be no injury or slightly injured, worse than that then the attempt is failed.

The reason why you need to sleep up to 10 hours is every hour of sleep will consume the eaten and burned calories in exchange of a bit of gain, if one of the calorie values are used up, the gain stops. The 9th and 10th provides 50% more gain.

Regular maximum muscle gain is 0.07kg for a 10 hours sleep. While the theoratical maximum muscle gain is 0.084kg, it's pretty hard to meet the extra calories targets.

### Muscle Reduction

These conditions decides if muscle reduction happens (checks every 22 hours by default):

- Lack of sleep (<= 4 hrs in 24 hrs)
- Insufficient eaten calories (The target is quite low so not a problem unless you are really out of food)
- Insufficient burned calories (The target is quite low so not a problem unless you hibernates)
- Low condition

Some of the afflictions will very slightly raise the calories target:

- Hypothermia
- Infection
- IntestinalParasites
- Broken Ribs

Usually the muscle reduce by 0.02kg/22hrs (by default), but in unlikely situations like all the conditions are unmet it could be up to 0.08kg.

Noted that gained muslce is taken into account for calories targets, so before the hard limits configured in the mod settings, you may hit a soft cap where you can only maintain your muscle and has trouble to gain more.

## Dependencies

- [Moment](https://github.com/No3371/TLD-Moment)
- [ModData](https://github.com/dommrogers/ModData/)
- [ModSettings](https://github.com/zeobviouslyfakeacc/ModSettings/)
