Import-Module "$PSScriptRoot/../bin/Debug/pwsh-prompt/pwsh-prompt.psd1" -Force -DisableNameChecking -ErrorAction SilentlyContinue

$Artists = @(
	@{ Value = "100 gecs";              HelpMessage = "Hyperpop, noise, absurdist electronic" }
	@{ Value = "A Tribe Called Quest";  HelpMessage = "Jazz rap, conscious hip-hop, golden era" }
	@{ Value = "Alice Coltrane";        HelpMessage = "Spiritual jazz, harp, devotional" }
	@{ Value = "Allen Stone";           HelpMessage = "Blue-eyed soul, funk, neo-soul" }
	@{ Value = "Amy Winehouse";         HelpMessage = "Soul, jazz, retro-pop, British" }
	@{ Value = "Anderson .Paak";        HelpMessage = "Funk, soul, hip-hop, drumming" }
	@{ Value = "Aphex Twin";            HelpMessage = "IDM, ambient techno, acid" }
	@{ Value = "Aretha Franklin";       HelpMessage = "Soul, gospel, R&B, Queen of Soul"; HotKey = "a" }
	@{ Value = "Beach House";           HelpMessage = "Dream pop, shoegaze, hypnotic" }
	@{ Value = "Beethoven";             HelpMessage = "Classical, symphonic, romantic era" }
	@{ Value = "Beirut";                HelpMessage = "Indie pop, Balkan brass, world folk" }
	@{ Value = "Billie Eilish";         HelpMessage = "Dark pop, electropop, whisper vocals" }
	@{ Value = "Bilmuri";               HelpMessage = "Post-hardcore, math rock, experimental" }
	@{ Value = "Bjork";                 HelpMessage = "Art pop, electronic, avant-garde" }
	@{ Value = "Boards of Canada";      HelpMessage = "Ambient, downtempo, hauntology" }
	@{ Value = "Bob Marley";            HelpMessage = "Reggae, roots, ska, one love"; HotKey = "b" }
	@{ Value = "Bon Iver";              HelpMessage = "Indie folk, experimental, falsetto" }
	@{ Value = "Breathe Carolina";      HelpMessage = "Electropop, EDM, post-hardcore roots" }
	@{ Value = "Brian Eno";             HelpMessage = "Ambient, generative, art rock" }
	@{ Value = "Bring Me the Horizon";  HelpMessage = "Metalcore, post-hardcore, electronic rock" }
	@{ Value = "Burial";                HelpMessage = "UK garage, dubstep, nocturnal ambience" }
	@{ Value = "Caribou";               HelpMessage = "Indie electronic, psychedelic, warm beats" }
	@{ Value = "Caroline Polachek";     HelpMessage = "Art pop, hyperpop, operatic" }
	@{ Value = "Charli XCX";            HelpMessage = "Hyperpop, electropop, PC Music" }
	@{ Value = "Childish Gambino";      HelpMessage = "Funk, soul, hip-hop, R&B polymath" }
	@{ Value = "Chopin";                HelpMessage = "Classical, romantic piano, nocturnes" }
	@{ Value = "Chris Brown";           HelpMessage = "R&B, pop, dance, hip-hop" }
	@{ Value = "Chris Stapleton";       HelpMessage = "Country, blues rock, Southern soul" }
	@{ Value = "Cocteau Twins";         HelpMessage = "Dream pop, ethereal wave, shoegaze" }
	@{ Value = "D'Angelo";              HelpMessage = "Neo-soul, funk, R&B classicist" }
	@{ Value = "Daft Punk";             HelpMessage = "French house, electro-funk, disco"; HotKey = "d" }
	@{ Value = "Dave Matthews Band";    HelpMessage = "Jam band, folk rock, jazz fusion" }
	@{ Value = "David Bowie";           HelpMessage = "Glam rock, art rock, reinvention" }
	@{ Value = "Dayseeker";             HelpMessage = "Post-hardcore, alternative rock, emotional" }
	@{ Value = "Debussy";               HelpMessage = "Classical, impressionist, orchestral" }
	@{ Value = "Depeche Mode";          HelpMessage = "Synth-pop, darkwave, electronic rock" }
	@{ Value = "Elliott Smith";         HelpMessage = "Indie folk, lo-fi, whisper-quiet" }
	@{ Value = "Emarosa";               HelpMessage = "Post-hardcore, alt-rock, melodic" }
	@{ Value = "Erykah Badu";           HelpMessage = "Neo-soul, jazz, conscious R&B" }
	@{ Value = "Fela Kuti";             HelpMessage = "Afrobeat, political music, Lagos grooves" }
	@{ Value = "Fever Ray";             HelpMessage = "Dark electro, synth, eerie pop" }
	@{ Value = "Fiona Apple";           HelpMessage = "Art pop, piano rock, confessional" }
	@{ Value = "FKA twigs";             HelpMessage = "Art pop, electronic, avant-R&B" }
	@{ Value = "Fleetwood Mac";         HelpMessage = "Soft rock, blues rock, California sound" }
	@{ Value = "Flying Lotus";          HelpMessage = "Experimental hip-hop, jazz, beat scene" }
	@{ Value = "Four Tet";              HelpMessage = "Folktronica, minimal techno, organic beats" }
	@{ Value = "Frank Ocean";           HelpMessage = "Alt-R&B, neo-soul, introspective" }
	@{ Value = "Gorillaz";              HelpMessage = "Trip-hop, alt-rock, virtual band" }
	@{ Value = "Grimes";                HelpMessage = "Art pop, synth-pop, cyberpunk" }
	@{ Value = "Herbie Hancock";        HelpMessage = "Jazz-funk, fusion, electronic jazz" }
	@{ Value = "J Dilla";               HelpMessage = "Lo-fi hip-hop, beat-making, jazz samples" }
	@{ Value = "James Brown";           HelpMessage = "Funk, soul, Godfather of Soul"; HotKey = "j" }
	@{ Value = "Janelle Monae";         HelpMessage = "Funk, afrofuturism, sci-fi soul" }
	@{ Value = "Jeff Buckley";          HelpMessage = "Alt-rock, folk rock, vocal range" }
	@{ Value = "John Coltrane";         HelpMessage = "Free jazz, modal jazz, spiritual" }
	@{ Value = "Joni Mitchell";         HelpMessage = "Folk, jazz-folk, confessional songwriting" }
	@{ Value = "Joy Division";          HelpMessage = "Post-punk, gothic, cold wave" }
	@{ Value = "JPEG Mafia";            HelpMessage = "Experimental hip-hop, glitch, noise rap" }
	@{ Value = "Kanye West";            HelpMessage = "Hip-hop, maximalist, gospel rap" }
	@{ Value = "Kate Bush";             HelpMessage = "Art pop, progressive, theatrical" }
	@{ Value = "Kelela";                HelpMessage = "Electronic R&B, club, futuristic" }
	@{ Value = "Kendrick Lamar";        HelpMessage = "Conscious hip-hop, jazz rap, West Coast"; HotKey = "k" }
	@{ Value = "King Crimson";          HelpMessage = "Progressive rock, avant-garde, math rock" }
	@{ Value = "King Gizzard";          HelpMessage = "Psych-rock, thrash, microtonal, prolific" }
	@{ Value = "Kings of Leon";         HelpMessage = "Southern rock, alt-rock, arena rock" }
	@{ Value = "Kraftwerk";             HelpMessage = "Electronic, synth-pop, Krautrock pioneers" }
	@{ Value = "Lauryn Hill";           HelpMessage = "Hip-hop, neo-soul, reggae fusion" }
	@{ Value = "LCD Soundsystem";       HelpMessage = "Dance-punk, electronic rock, DFA" }
	@{ Value = "Led Zeppelin";          HelpMessage = "Hard rock, blues rock, heavy metal roots"; HotKey = "z" }
	@{ Value = "Little Simz";           HelpMessage = "UK hip-hop, grime, introspective bars" }
	@{ Value = "M83";                   HelpMessage = "Shoegaze, electronic, cinematic synth-pop" }
	@{ Value = "Mac DeMarco";           HelpMessage = "Slacker rock, jangle pop, lo-fi indie" }
	@{ Value = "Madlib";                HelpMessage = "Abstract beats, jazz samples, crate digging" }
	@{ Value = "Marc Broussard";        HelpMessage = "Bayou soul, blue-eyed soul, R&B" }
	@{ Value = "Mariah the Scientist";  HelpMessage = "Alt-R&B, dark pop, introspective" }
	@{ Value = "Marvin Sapp";           HelpMessage = "Gospel, contemporary Christian, worship" }
	@{ Value = "Massive Attack";        HelpMessage = "Trip-hop, downtempo, Bristol sound" }
	@{ Value = "Matisyahu";             HelpMessage = "Reggae, hip-hop, Hasidic Jewish roots" }
	@{ Value = "MF DOOM";               HelpMessage = "Abstract hip-hop, villain rap, wordplay" }
	@{ Value = "Miguel";                HelpMessage = "R&B, funk, alt-pop, falsetto" }
	@{ Value = "Miles Davis";           HelpMessage = "Jazz, cool jazz, fusion, modal"; HotKey = "m" }
	@{ Value = "Mitski";                HelpMessage = "Indie rock, art pop, emotional catharsis" }
	@{ Value = "Modest Mouse";          HelpMessage = "Indie rock, post-punk, angular guitars" }
	@{ Value = "Mozart";                HelpMessage = "Classical, symphonic, opera, prodigy" }
	@{ Value = "Mulatu Astatke";        HelpMessage = "Ethio-jazz, vibraharp, world fusion" }
	@{ Value = "My Bloody Valentine";   HelpMessage = "Shoegaze, noise pop, wall of sound" }
	@{ Value = "Nas";                   HelpMessage = "East Coast hip-hop, lyrical storytelling" }
	@{ Value = "New Order";             HelpMessage = "Synth-pop, post-punk, dance-rock" }
	@{ Value = "Nick Drake";            HelpMessage = "Folk, chamber pop, pastoral" }
	@{ Value = "Nickel Creek";          HelpMessage = "Progressive bluegrass, newgrass, folk" }
	@{ Value = "Nina Simone";           HelpMessage = "Jazz, soul, blues, civil rights anthems" }
	@{ Value = "Nujabes";               HelpMessage = "Jazz hip-hop, lo-fi, instrumental" }
	@{ Value = "Olivia Dean";           HelpMessage = "Neo-soul, pop, warm British vocals" }
	@{ Value = "OutKast";               HelpMessage = "Southern hip-hop, funk, ATL innovation" }
	@{ Value = "Phoebe Bridgers";       HelpMessage = "Indie folk, sad rock, introspective" }
	@{ Value = "Pink Floyd";            HelpMessage = "Progressive rock, psychedelic, concept albums"; HotKey = "f" }
	@{ Value = "Pixies";                HelpMessage = "Alt-rock, noise pop, loud-quiet-loud" }
	@{ Value = "Portishead";            HelpMessage = "Trip-hop, downtempo, dark electronica" }
	@{ Value = "Prince";                HelpMessage = "Funk, pop, rock, Minneapolis Sound"; HotKey = "p" }
	@{ Value = "Radiohead";             HelpMessage = "Alt-rock, experimental, electronic"; HotKey = "r" }
	@{ Value = "Ray Charles";           HelpMessage = "Soul, R&B, blues, piano genius" }
	@{ Value = "Rihanna";               HelpMessage = "Pop, dancehall, R&B, Barbadian" }
	@{ Value = "Run the Jewels";        HelpMessage = "Political rap, boom-bap, industrial hip-hop" }
	@{ Value = "Ryuichi Sakamoto";      HelpMessage = "Electronic, ambient, classical, film scores" }
	@{ Value = "Sade";                  HelpMessage = "Smooth jazz, quiet storm, sophisti-pop" }
	@{ Value = "Shabaka Hutchings";     HelpMessage = "Nu-jazz, spiritual jazz, London scene" }
	@{ Value = "Sigur Ros";             HelpMessage = "Post-rock, ambient, Icelandic" }
	@{ Value = "Slowdive";              HelpMessage = "Shoegaze, dream pop, ambient" }
	@{ Value = "Solange";               HelpMessage = "Alt-R&B, art pop, neo-soul" }
	@{ Value = "Sonic Youth";           HelpMessage = "Noise rock, alt-rock, guitar tunings" }
	@{ Value = "St. Vincent";           HelpMessage = "Art rock, indie pop, guitar innovation" }
	@{ Value = "Stevie Wonder";         HelpMessage = "Soul, funk, R&B, harmonica virtuoso" }
	@{ Value = "Sufjan Stevens";        HelpMessage = "Indie folk, orchestral pop, baroque" }
	@{ Value = "SZA";                   HelpMessage = "Alt-R&B, neo-soul, confessional" }
	@{ Value = "Talking Heads";         HelpMessage = "New wave, post-punk, art funk" }
	@{ Value = "Tame Impala";           HelpMessage = "Psychedelic pop, synth-rock, neo-psych" }
	@{ Value = "Tangerine Dream";       HelpMessage = "Berlin school, sequencer music, space electronic" }
	@{ Value = "Taylor Dayne";          HelpMessage = "Pop, dance-pop, freestyle, power ballads" }
	@{ Value = "The 1975";              HelpMessage = "Indie pop, synth-pop, alt-rock" }
	@{ Value = "The Beatles";           HelpMessage = "Rock, pop, psychedelia, British Invasion"; HotKey = "t" }
	@{ Value = "The Chemical Brothers"; HelpMessage = "Big beat, electronica, rave" }
	@{ Value = "The Cure";              HelpMessage = "Gothic rock, new wave, post-punk pop" }
	@{ Value = "The Home Team";         HelpMessage = "Pop-punk, post-hardcore, melodic" }
	@{ Value = "The Midnight";          HelpMessage = "Synthwave, retrowave, nocturnal pop" }
	@{ Value = "The Prodigy";           HelpMessage = "Big beat, breakbeat, rave anthems" }
	@{ Value = "The Smiths";            HelpMessage = "Indie rock, jangle pop, post-punk" }
	@{ Value = "The Supremes";          HelpMessage = "Motown, soul, girl group, Diana Ross" }
	@{ Value = "The Weeknd";            HelpMessage = "Dark R&B, synth-pop, alt-pop" }
	@{ Value = "The Whispers";          HelpMessage = "R&B, soul, quiet storm, disco" }
	@{ Value = "Thundercat";            HelpMessage = "Jazz fusion, funk, bass virtuoso" }
	@{ Value = "Tinariwen";             HelpMessage = "Desert blues, Tuareg guitar, Saharan" }
	@{ Value = "Tom Waits";             HelpMessage = "Experimental, blues, gravelly storytelling" }
	@{ Value = "Tyler, the Creator";    HelpMessage = "Hip-hop, neo-soul, jazz rap" }
	@{ Value = "Vivaldi";               HelpMessage = "Classical, baroque, The Four Seasons" }
	@{ Value = "Wale";                  HelpMessage = "Hip-hop, go-go, conscious rap, D.C." }
	@{ Value = "Weyes Blood";           HelpMessage = "Baroque pop, chamber folk, cinematic" }
	@{ Value = "Wind Walkers";          HelpMessage = "Post-hardcore, metalcore, melodic" }
	@{ Value = "Wu-Tang Clan";          HelpMessage = "East Coast hip-hop, martial arts rap"; HotKey = "w" }
)

$BufferConfig = @{

}

$Result = Prompt-Choice $Artists `
	-Message @{ Text = "Select the artists you are interested in"; ForegroundColor = [Colors]::WHITE; Style = "Italic" } `
	-Title @{ Text = "▃▇▅▁▃▆▇▄▂ Musical Interests"; Style = "Bold" } `
	-Default 3 `
	-Multiple `
	-AlternateBuffer $BufferConfig

if ($null -eq $Result) {
	Write-Host "`n  Cancelled!" -ForegroundColor DarkGray
	return
}

Write-Host ""
Write-Host "  🎶 Your picks:" -ForegroundColor Magenta
foreach ($i in $Result) {
	$Artist = $Artists[$i].Value
	Write-Host "     $Artist" -ForegroundColor Green
}
Write-Host ""
