# Google Text-to-Speech wrapper  
This is wrapper for Google Text-to-Speech service:  
https://cloud.google.com/text-to-speech
  
This is a console application which helps to create many speeches (saved as mp3 files) from one text file.  
Application uses .NET Core 3.1 and is written in C# language.

To use this program you need to create account on Google Cloud Platform and activate the Text-to-Speech API.  
***The first 1 million characters are free each month.***  

Next you must create a service account, generate a key and download it on your computer.  
Final step is setting the environment variable which contains the path to that file.  
See: https://cloud.google.com/docs/authentication/getting-started   

## Command-line interface
First parameter is required. This parameter should be a path to the text file to synthesize.  
Next parameters are optional:
- **\-\-one-file** or **\-1** - if this option occurs, the program is invoked in one file mode, otherwise the application is invoked in bulk file mode. Those two modes are described below.
- **\-\-voice \<text\>** or **\-V \<text\>** - this parameter sets the voice in google text service. List of all available voices is here: https://cloud.google.com/text-to-speech/docs/voices

Here are three parameters which are used in bulk mode:
- **\-\-index-format \<integer\>** or **\-F \<integer\>** - parameter describes format and trailing zeros of the index. E.g **\-F 3** indexes look like that: "001", "023". Default: 2
- **\-\-index-separator \<text\>** or **\-S \<text\>** - parameter describes separator between chapter and subchapter names. Default: \_
- **\-\-chapter-separator \<text\>** or **\-C \<text\>** - parameter describes separator between chapter name and index. Default: \_

## One file mode
In this mode program reads text file and synthesizes all content of this file to one mp3 file.

## Bulk file mode
***To understand this mode and markups check samples which are placed in directory: "TextToSpeech.Tests/Samples"***  

Content from the text file is splitted to many sections. Each section is separated by at least one empty line.  
Application for every section synthesizes one mp3 file with a unique name.  
Users can use the following markups to provide desired file names for generated files.  

Generated mp3 file names take this form:  
\<chapter\>\<chapter-separator\>\<subchapter #1\>\<chapter-separator\>\<subchapter #2\>.mp3  
Of course users can add so many subchapters if they want.  

If two sections have the same name based on chapter/subchapters than index will be added:  
\<chapter\>\<chapter-separator\>\<subchapter #1\>\<index-separator\>\<index\>.mp3  
Index is minimal number, to achieve unique name, but not less then start index (see @$ markup)    

### Markups
- \# \<text\> - comment. Every line which stars from \# is treated like a comment and will be ignored.  

Those markup are applied for section where occur and further sections:
- @+ \<text\> - add chapter/subchapter name. \<text\> should be not white spaces.
- @@ \<text\> - replace subchapter name. \<text\> should be not white spaces.
- @\-, @\-\-, @\-\-\-, etc. - remove subchapter names. It counts the minuses. E.g. @\-\-\- removes 3 subchapter names.
- @\-\-@ \<text\> - syntactic sugar for lines: @\-\- and @@ \<text\>
- @$ \<integer\> - start index. Sometimes to provide unique file names an index is added. For those sections the index won’t be less than the provided number.

Those markup are applied only for one section:
- @^ \<text\> - custom section name. For that section it takes into account only provided text, not takes chapter and subchapters. \<text\> should be not white spaces. 
- @\! \<text\> - synthesize only. If there are sections in the bulk file with that markup, only those sections will be synthetized, but for every section the unique name will be computed. 
It is very helpful when we notice mistakes in some sections and want to fix it. 
- @\# \<text\> - ignored sections. Sections with those markup won’t be synthesized, but a unique name will be reserved for that section. 
It can’t be used with markup @\!, otherwise application will be stopped.
