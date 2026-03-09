using UnityEngine;

public class GrokCollabRules : MonoBehaviour
{
	/*
	 * https://grok.com/project/a3676310-4a2c-4197-93a2-c9af20223edd?chat=f748c62f-f5de-4c07-bd1f-23c841e502f3&rid=3a2326a6-5740-4119-8195-179977d7c13a
	 * 
	 * 
	 * 
	 *	// ----- ----- ----- USER INSTRUCTIONS ----- ----- ----- :
	
			We need to set base rules for how we work together.
 
			First base rule:
			Understanding the code is neccesary for me and others. To understand the code, it needs to be consistent. To Achieve this
			 
			Minimum abbreviations should be used, and names need to be as descriptive as possible.
			 
			All functions and blocks of variables and assignments should be named and arranged with descriptions of there purposes.
			 
			Primary values that can be tweaked such as speed, max collision angle, and other values should be marked as "PARAMETERS"
			 
			Arrangement of code should not be drastically changed. Although it can be helpful, consistency in the spacing, arrangment and styling is important to understand the changes	you		provide and how they affect the broader script. Changing names and assignemnt could also negatively affect other scipts that are referenced outside of this script,			introducting	 errors. Keep as much as possible the same.
			 
			Mark changes to the code with text decoration marking changes and introductions.
			 
			Please repeat these instructions. Summarize them, and state why they are important.




			// ----- ----- ----- GROK RESPONSE ----- ----- ----- :

			Here are your base rules repeated exactly as you provided them:

			> We need to set base rules for how we work together.
			> 
			> First base rule:
			> Understanding the code is neccesary for me and others. To understand the code, it needs to be consistent. To Achieve this
			> 
			> Minimum abbreviations should be used, and names need to be as descriptive as possible.
			> 
			> All functions and blocks of variables and assignments should be named and arranged with descriptions of there purposes.
			> 
			> Primary values that can be tweaked such as speed, max collision angle, and other values should be marked as "PARAMETERS"
			> 
			> Arrangement of code should not be drastically changed. Although it can be helpful, consistency in the spacing, arrangment and styling is important to understand the changes		you	provide and how they affect the broader script. Changing names and assignemnt could also negatively affect other scipts that are referenced outside of this script,			introducting	 errors. Keep as much as possible the same.
			> 
			> Mark changes to the code with text decoration marking changes and introductions.
			
			### Summary of the rules (in clearer language)
			
			1. **Prioritize readability and consistency**  
			   - Use very few abbreviations  
			   - Choose long, descriptive, self-explanatory names for variables, methods, and sections
			
			2. **Add clear purpose documentation**  
			   - Every method should have a descriptive name  
			   - Group related variables/assignments into clearly named blocks  
			   - Include comments explaining the purpose of each significant block or function
			
			3. **Clearly mark tunable values**  
			   - All important tweakable numbers (speeds, angles, friction, restitution, etc.) should be grouped and labeled as **PARAMETERS** (usually in the Inspector via `		[ SerializeField]` + header or comment)
			
			4. **Preserve existing code structure as much as possible**  
			   - Do not reorganize large sections of the file  
			   - Keep similar spacing, indentation style, and overall layout  
			   - Avoid renaming existing variables/methods unless absolutely necessary (to prevent breaking references from other scripts)
			
			5. **Clearly indicate every change**  
			   - When showing modified code, use visible markup (e.g. comments like `// CHANGED: …`, `// NEW: …`, `// REMOVED: …`) to highlight what was added, modified, or deleted
			
			### Why these rules are important
			
			- **You (and potentially other people)** need to be able to quickly understand what the code does months from now — without having to reverse-engineer cryptic names or guess		why		a		value exists.
			- **Consistency reduces cognitive load** — sudden style/format/name changes make diffs harder to read and increase the chance of missing important behavior changes.
			- **Preserving structure protects other parts of the project** — many Unity scripts reference each other by exact method/variable names. Renaming things silently can silently			break paddle input, UI updates, power-up systems, sound triggers, etc.
			- **Marked changes make collaboration & debugging faster** — you can instantly see “what Grok touched this time” instead of doing a full code review to spot differences.
			- **Clearly labeled PARAMETERS** let non-programmers (or future you) tune feel without touching logic — critical for game feel iteration in Breakout-style games.
			
			I understand and will follow these rules going forward.
			
			From now on when I suggest code changes I will:
			- Keep the overall file layout and method ordering very close to what you last shared
			- Use descriptive names (avoiding abbreviations like `vThis` → `velocityOfThisBall`, etc. when practical)
			- Label tweakable values clearly as **PARAMETERS**
			- Add purpose comments to new/modified blocks
			- Use explicit change markers (`// CHANGED:`, `// ADDED:`, `// REMOVED:` etc.)
			
			Ready for the next piece — whether that’s paddle input, power-ups, level generation, scoring, lives system, or anything else.
			
			What would you like to work on next?


		*/
}
