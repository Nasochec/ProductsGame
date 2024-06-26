% !TeX encoding = windows-1251
%% 
%% This is file diplom.cls
%% It is based on report.cls
%% 
%% Developed by: Aleksandr Maksimenko, maximenko.a.n@gmail.com
%%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
\NeedsTeXFormat{LaTeX2e}
\ProvidesClass{diplom}[2017/01/25 Provides the format for diploms]
% ��������� report.cls �� ����� �������, ����������� � diplom
\LoadClassWithOptions{report} 

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ���������� �������� ������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\input{glyphtounicode.tex} % ���������� ��������� �������� � PDF
\pdfgentounicode=1
%\RequirePackage{cmap} % ���������� ��������� �������� � PDF (�����.)
\RequirePackage[T2A]{fontenc} % ���������� ��������� TeX-�
\RequirePackage[cp1251]{inputenc} % ��������� �����
\RequirePackage[english,russian]{babel} % �����������, ������� ��������� � �.�.

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ����������� ����� ����������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\RequirePackage[dotinlabels]{titletoc} % ����� � ������� �������� � ����������
\RequirePackage[newlinetospace]{titlesec} % ����� newlinetospace �������� \\ �� ������� � ����������
%\titlelabel{\thetitle.\quad} % ����� � ������� ��������
\newcommand{\TitleLabelWidth}{2cm} % ������ ����� ��� ����� ��������� (������� �� ��������)
\titleformat{\chapter}[hang]{%
	\filright\normalfont\Large\bfseries% The format of the whole title: label and text
	}{\stepcounter{chapternum}% ����������� ����������� ������� ����
	\makebox[\TitleLabelWidth][l]{\thechapter.}% The label
	}{0em}{}
\titleformat{\section}
{\normalfont\large\bfseries}{\makebox[\TitleLabelWidth][l]{\thesection.}}{0em}{}
\titleformat{\subsection}
{\normalfont\normalsize\bfseries}{\makebox[\TitleLabelWidth][l]{\thesubsection.}}{0em}{}
\titleformat{\subsubsection}
{\normalfont\normalsize\bfseries}{\makebox[\TitleLabelWidth][l]{\thesubsubsection.}}{0em}{}

% The standart definition of chapter and sections title format
%\titleformat{\chapter}[display]
%{\normalfont\huge\bfseries}{\chaptertitlename\ \thechapter.}{20pt}{\Huge}
%\titleformat{\section}
%{\normalfont\Large\bfseries}{\thesection}{1em}{}
%\titleformat{\subsection}
%{\normalfont\large\bfseries}{\thesubsection}{1em}{}
%\titleformat{\subsubsection}
%{\normalfont\normalsize\bfseries}{\thesubsubsection}{1em}{}

% \makebox[2cm][l]{\thetitle.} ������ ����� ������������� ������
%\titlelabel{\makebox[2cm][l]{\thetitle.}}
\titlespacing{\chapter}{0pt}{1em}{*3.5} % ������������ ������� ������ ���������
%\titlespacing{\section}{0pt}{1em}{\aftertitleunit}
%\titlespacing{\subsection}{0pt}{1em}{\aftertitleunit}
%\titlespacing{\subsubsection}{0pt}{1em}{\aftertitleunit}

% ������� ������� ��� �������������� �����
\newcommand*{\chapternonum}[1]{\chapter*{#1}\addcontentsline{toc}{chapter}{#1}}

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ����� ���������� � �����������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% ����� appendix ������ � ��������� �������������
%\usepackage[titletoc]{appendix}

\AtBeginDocument{\let\oldappendix=\appendix
	\def\appendix{%
		%\setcounter{chapternum}{\value{chapter}}
		\oldappendix
		% ��������� ���������� �������� �������
		\renewcommand{\thechapter}{\Asbuk{chapter}}
		% ���������� ��������� ����������
		\titleformat{\chapter}[display]%
		{\normalfont\Large\bfseries% The format of label and text
		}{\stepcounter{appnum}% ����������� ����������� ������� ����������
		\filleft\normalfont\Large\chaptertitlename\ \thechapter% The label
		}{0pt}{\filright}
		\titlespacing{\chapter}{0pt}{0pt}{*3.5}
		% ��������� ����� ���������� � ������� ���� � ����������
%		\titlecontents{chapter}% <section-type>
%			[0pt]% <left>
%			{\vspace{1em}\bfseries}% <above-code>
%			{\appendixname\ \thecontentslabel.\enspace}% <numbered-entry-format>
%			{}% <numberless-entry-format>
%			{\hfill\contentspage}% <filler-page-format>
			%[\vspace{2pt}]% <below-code>
		% ��������� ����� ���������� � ������ ������ � ����������
		\let\addcontentslineold=\addcontentsline
		\renewcommand{\addcontentsline}[3]{\addcontentslineold{##1}{##2}{\appendixname~##3}}
	}
}


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ���������� ����������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% ����������
\AtBeginDocument{\let\oldtableofcontents=\tableofcontents
	\renewcommand{\tableofcontents}[1][����������]{%
		\renewcommand{\contentsname}{#1}
		\oldtableofcontents}
}

\RequirePackage[nottoc]{tocbibind} % ��������� ������ ���������� � ����������
% ��� ��������� ��������� ������ ����� ����� \begin{thebibliography}{9}
%\addcontentsline{toc}{chapter}{\bibname} % ��������� � ����������


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% �������� ��� ���������� ������ ������������ ������ "~ � ��������� ���� "--
%% russianb.ldf        begin
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
\newcommand*{\glue}{\nobreak\hskip\z@skip}%  NEW!!!
%\declare@shorthand{russian}{"~}{\textormath{\leavevmode\hbox{-}}{-}}%  OLD!!!
\declare@shorthand{russian}{"~}{\glue\hbox{-}\glue}%  NEW!!!
\def\cdash#1#2#3{\def\tempx@{#3}%
	\def\tempa@{-}\def\tempb@{~}\def\tempc@{*}%
	\ifx\tempx@\tempa@\@Acdash\else
	\ifx\tempx@\tempb@\@Bcdash\else
	\ifx\tempx@\tempc@\@Ccdash\else
	%\errmessage{Wrong usage of cdash}%  OLD!!!
	\@Dcdash#3\fi\fi\fi}%  NEW!!!
%\def\@Acdash{\ifdim\lastskip>\z@\unskip\nobreak\hskip.2em\fi
%  \cyrdash\hskip.2em\ignorespaces}%
%\def\@Bcdash{\leavevmode\ifdim\lastskip>\z@\unskip\fi%  OLD!!!
% \nobreak\cyrdash\penalty\exhyphenpenalty\hskip\z@skip\ignorespaces}%  OLD!!!
%\def\@Ccdash{\leavevmode
% \nobreak\cyrdash\nobreak\hskip.35em\ignorespaces}%
\def\@Bcdash{\,\textendash\,\hskip\z@skip\ignorespaces}%  NEW!!!
\def\@Dcdash#1{\,\textendash\,\hskip\z@skip\ignorespaces#1}%  NEW!!!
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% russianb.ldf        end
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%




%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ������� ��� �������� � ������ 
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\RequirePackage[justification=centering, margin=2em, labelfont=bf]{caption}
\DeclareCaptionLabelSeparator*{emdash}{~--- }
\captionsetup{labelsep=emdash, figurewithin=none}
%\DeclareCaptionFormat{rustable}{\hfill #1\\[3pt]\centering\bf{}#3\par}
%format=rustable
\DeclareCaptionLabelFormat{rtable}{\hfill #1~#2}
\captionsetup[table]{labelformat=rtable, labelsep=newline, labelfont=normalfont, textfont=bf, position=above, margin=0em}
%\intextsep=8pt % ������� ������ � ����� ������� (�������)


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ����������� ���������� �������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\RequirePackage{enumitem} 
\setlist{noitemsep, leftmargin=\parindent}
\setlist[itemize]{label=--}


%\RequirePackage[overload]{textcase} % Some upgrade for UpperCase

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ��������� ��������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% ���������� ���������� ��� ��������� ��������� ��������
\newcommand*{\LetterHead}[1]{\gdef\@LetterHead{#1}}
\newcommand*{\Kafedra}[1]{\gdef\@Kafedra{#1}}
\newcommand*{\ZavName}[1]{\gdef\@ChairName{#1}}
\newcommand*{\ZavStatus}[1]{\gdef\@ChairStatus{#1}}
\newcommand*{\ZavKaf}[2]{\gdef\@ChairStatus{#1}\gdef\@ChairName{#2}}
\newcommand*{\OverTitle}[1]{\gdef\@DocumentType{#1}}
\newcommand*{\DocumentType}[1]{\gdef\@DocumentType{#1}}
\newcommand*{\Napr}[1]{\gdef\@Napr{#1}}
\newcommand*{\Title}[1]{\gdef\@Title{#1}}
\newcommand*{\AuthorName}[1]{\gdef\@AuthorName{#1}}
\newcommand*{\AuthorStatus}[1]{\gdef\@AuthorStatus{#1}}
\newcommand*{\Author}[2]{\gdef\@AuthorStatus{#1}\gdef\@AuthorName{#2}}
\newcommand*{\ChiefName}[1]{\gdef\@ChiefName{#1}}
\newcommand*{\ChiefStatus}[1]{\gdef\@ChiefStatus{#1}}
\newcommand*{\Chief}[2]{\gdef\@ChiefStatus{#1}\gdef\@ChiefName{#2}}
\newcommand*{\City}[1]{\gdef\@City{#1}}
\newcommand*{\Year}[1]{\gdef\@Year{#1}}

\newif\ifkursovaya % ���� ��� ��������
\kursovayafalse % �� ��������� -- �� ��������
\newcommand*{\Kursovaya}{\kursovayatrue}

% �������������� �������� ��������� ��������
\LetterHead{������������ ����� � ������� ����������� ���������� ��������� \\
	����������� ��������������� ��������� ���������������\\ ���������� ������� �����������\\
	<<����������� ��������������� ����������� ����� �.\,�.~��������>>}
\Kafedra{������� ������������� �����������}
\Napr{����������� 02.03.02 ��������������� ����������� � �������������� ����������\\������� <<������������ �����>>}
\ZavStatus{���������� ��������,\\ �.\,�.-�.\,�., ���������}
\ZavName{�.\,�.~�������}
\DocumentType{\large ��������� ���������������� ������ ���������}
\Title{\Large\bfseries �������� ��������� ������}
\ChiefStatus{������� ������������\\ �.\,�.-�.\,�., ������}
\ChiefName{�.\,�.~������}
\AuthorStatus{������� ������ ��-41��}
\AuthorName{�.\,�.~������}
\City{���������}
\Year{\the\year} % this year

% ������� ��������� ��� ����� �������
%\newcommand*{\RightTitleWidth}{6cm} % ������ ������ �������
\newlength{\VisaWidth} % ���������� -- ������ ������ �������
\setlength{\VisaWidth}{6cm} % �������������� ������ ������ �������
\newcommand*{\SignDate}{\\[3pt]<<\rule{1cm}{0.5pt}>>\hrulefill\ \@Year~�.} % ����
\newcommand*{\SignBlock}[1]{%
   \vspace{0.5\baselineskip}
   \hbox to \textwidth{%
   \hfill%
   \begin{minipage}[t]{\VisaWidth}
     \begin{flushleft}
       #1
     \end{flushleft}
   \end{minipage}%
   \vspace{1.1\baselineskip}
   }%
}
\newcommand*{\StandardSign}[2]{%
    \SignBlock{#1\\[\baselineskip]
	\hrulefill\ #2
	\SignDate}
}

% ����������� �������� ��������� ��������
\renewcommand*{\maketitle}{%
\begin{titlepage}
	% ���� ����� hyperref ���������,	
	% ��������� � �������� PDF-��������� �������� � ������
	\@ifundefined{hypersetup}{}{
		\hypersetup{pdftitle = \@Title, %pdfsubject,
			pdfauthor = \@AuthorName%, pdfkeywords
		}
	}
	%\@ifpackageloaded{hyperref}{}{}
	%\thispagestyle{empty}
	\begin{center}
	  %\small 
	  {\@LetterHead}\\[0.5\baselineskip]
	  {\@Kafedra}\\
	 
	\end{center}
	\ifkursovaya
		\vfill
	\else
		%\StandardSign{<<��������� � ������>>\\ \@ChairStatus}{\@ChairName}
		\SignBlock{����� �� ������� \SignDate\\
			\@ChairStatus\\[\baselineskip]
			\hrulefill\ \@ChairName%
		}
	\fi  
	\begin{center} % ������ ������ ����� ��� ����, ����� ��� ������������ ������ ���������� ����������� ���������
	    {\@DocumentType
	    
	    }\vspace{\baselineskip}
		{\@Title
	
		}\vspace{\baselineskip}
		{\@Napr
		%{\@Napr}\\
		}%\vspace{\baselineskip}
	\end{center} 
	
	\StandardSign{\@ChiefStatus}{\@ChiefName}
	\StandardSign{\@AuthorStatus}{\@AuthorName}
	\vfill\strut
	\centerline{\@City, \@Year}
\end{titlepage}
\setcounter{page}{2}
}


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ���������� �������� ����
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\newcommand*{\Keywords}[1]{%
	�������� �����: \textbf{#1}
	% ����� pdfkeywords ��� hyperref �������� ������ � ����� ������ ���������
	%\@ifundefined{hypersetup}{}{\hypersetup{pdfkeywords={#1}}}
	}

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% �������� ��� ������ ������ ������������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\RequirePackage{totcount}
\regtotcounter{page} % Total number of pages
\newtotcounter{chapternum} % Total number of chapters
\newtotcounter{appnum} % Total number of apps
\newtotcounter{bibnum} % Total number of bibitems
\let\oldbibitem=\bibitem
\def\bibitem{\stepcounter{bibnum}\oldbibitem}

% � ������ ����� �������� �������� � ������ ����������.
% ������� ������� ����������� �������� ��� ������ �����
\newtotcounter{fignum} % Total number of figures
\newtotcounter{tablenum} % Total number of tables
\AtBeginDocument{%
	% ��������� ��� ������� ������ ���������� ��������������� ����� \begin{document}
	\let\oldcaption=\caption
	\def\caption{\def\floatf{figure}\ifx\floatf\@captype\stepcounter{fignum}\else\stepcounter{tablenum}\fi\oldcaption}%
}


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% �������������� �������, ������� � ��������� ���� ������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\RequirePackage{amsmath} % ������� ����� ��� ����������
\RequirePackage{amsthm}  % ��������� ���� ������
\RequirePackage{amssymb} % �������������� ������� � ������

\RequirePackage{mathtools} % �������������� ����������� ��� ������ ������
\providecommand\given{} % ��� ������������ �����, ������������ ������ �������
% ������� ������� ��� ���������� ��������
\newcommand\SetSymbol[1][]{%
	\nonscript\:#1\vert
	\allowbreak \nonscript\:	\mathopen{}}
\DeclarePairedDelimiterX\Set[1]\{\}{%
	\renewcommand\given{\SetSymbol[\delimsize]}	#1}
% �������� �����������
\DeclarePairedDelimiterXPP\Pb[1]{\mathsf{P}}(){}{
	\renewcommand\given{\nonscript\:\delimsize\vert\nonscript\:\mathopen{}}	#1}

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% ������������� �������� �������� � ������
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\RequirePackage{chngcntr}
%\counterwithout{figure}{chapter} % ���������� ������� �������� �� �������� ����
\counterwithout{equation}{chapter} % ���������� ������� ������ �� �������� ����
\let\numberwithin=\counterwithin % ����� \numberwithin (�� amsmath) � \counterwithin ����� �������������



%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
\endinput
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% End of file `diplom.cls'. 
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
