# LessonsHub UI Redesign — Task Brief for HTML/CSS Developer

## Project Overview

**LessonsHub** is an educational platform built with Angular 17+ (standalone components) and Angular Material. It helps users create AI-generated lesson plans, schedule lessons on a calendar, and study with interactive exercises and AI-reviewed answers.

The app currently has a functional but basic UI. The goal is to **redesign the visual appearance** (HTML structure + CSS styling) without changing the component logic, data bindings, or Angular Material directives.

---

## Tech Stack (Frontend)

- **Framework:** Angular 17+ (standalone components)
- **UI Library:** Angular Material (mat-card, mat-button, mat-icon, mat-select, mat-form-field, mat-expansion-panel, mat-datepicker, mat-dialog, mat-chips, etc.)
- **Markdown Rendering:** ngx-markdown (`<markdown [data]="...">`)
- **Styling:** Component-scoped CSS (no global SCSS/Tailwind)
- **Routing:** Angular Router with `routerLink`, `routerLinkActive`

---

## Application Structure

```
src/app/
├── app.ts / app.html / app.css          ← Root component (navbar + router-outlet)
├── app.routes.ts                         ← Route definitions
├── todays-lessons/                       ← Landing page — today's scheduled lessons
│   ├── todays-lessons.ts
│   ├── todays-lessons.html
│   └── todays-lessons.css
├── lesson-plan/                          ← AI lesson plan generator form + results
│   ├── lesson-plan.ts
│   ├── lesson-plan.html
│   └── lesson-plan.css
├── lesson-days/                          ← Calendar — assign lessons to dates
│   ├── lesson-days.ts
│   ├── lesson-days.html
│   └── lesson-days.css
├── lesson-detail/                        ← Single lesson view (content, exercises, answers)
│   ├── lesson-detail.ts
│   ├── lesson-detail.html
│   └── lesson-detail.css
├── models/                               ← TypeScript interfaces
└── services/                             ← HTTP services (do not modify)
```

---

## Routes

| Path | Component | Purpose |
|------|-----------|---------|
| `/today` (default) | TodaysLessons | Daily lesson schedule |
| `/lesson-plan` | LessonPlan | AI lesson plan generator |
| `/lesson-days` | LessonDays | Lesson calendar/scheduler |
| `/lesson/:id` | LessonDetail | Individual lesson view |

---

## Pages to Redesign

### 1. App Shell — `app.html` + `app.css`

**Current structure:**
- `<header>` with `<nav class="navbar">` containing brand link + 3 nav links
- `<main>` with `<router-outlet />`

**Current styling:**
- Purple gradient header (`#667eea → #764ba2`)
- White nav links with hover/active background
- Responsive: column layout on mobile

**What to keep:**
- `routerLink` and `routerLinkActive` directives on nav links
- `<router-outlet />` in main

**Redesign notes:**
- Consider a sidebar or improved top nav
- Add visual indicator for active route
- Ensure smooth responsive behavior

---

### 2. Today's Lessons — `todays-lessons.html` + `todays-lessons.css`

**Current structure:**
- Greeting header (dynamic: "Good morning/afternoon/evening")
- Date subtitle
- Loading state (mat-progress-bar)
- Error state (icon + message + retry button)
- Schedule card with lesson list (mat-list items, each links to `/lesson/:id`)
- Empty state (dashed border card with icon + "Schedule Lessons" button)
- "Manage Schedule" button at bottom

**Angular bindings to preserve:**
```
{{ greeting }}
{{ today | date:'fullDate' }}
*ngIf="isLoading"
*ngIf="error"
(click)="loadTodaysLessons()"
*ngIf="lessonDay && !isLoading && !error"
{{ lessonDay.name }}
{{ lessonDay.lessons.length }}
{{ lessonDay.shortDescription }}
*ngFor="let lesson of lessonDay.lessons"
[routerLink]="['/lesson', lesson.id]"
{{ lesson.name }}
{{ lesson.lessonNumber }}
{{ lesson.lessonPlanName }}
*ngIf="!lessonDay && !isLoading && !error"
routerLink="/lesson-days"
```

**Redesign notes:**
- This is the landing page — make it welcoming and informative
- Lesson cards should be easy to scan and tap/click
- Consider progress indicators or lesson status chips

---

### 3. Lesson Plan Generator — `lesson-plan.html` + `lesson-plan.css`

**Current structure:**
- Form card with:
  - Row 1: Lesson type select + Plan name input
  - Row 2: Number of lessons input + Topic input
  - Full-width: Description textarea
  - Error message display
  - Reset + Generate buttons
- Results card (after generation):
  - Plan name and topic as mat-chips
  - Success message
  - List of generated lesson cards (number badge + name + description)
  - Download JSON + Save to Database buttons

**Angular bindings to preserve:**
```
[(ngModel)]="lessonType"
[(ngModel)]="planName"
[(ngModel)]="numberOfDays"
[(ngModel)]="topic"
[(ngModel)]="description"
*ngIf="error"
(click)="resetForm()"
(click)="generateLessonPlan()"
[disabled]="isLoading"
*ngIf="isLoading"
*ngIf="generatedPlan"
{{ generatedPlan.planName }}
{{ generatedPlan.topic }}
{{ generatedPlan.lessons.length }}
*ngFor="let lesson of generatedPlan.lessons"
{{ lesson.lessonNumber }}
{{ lesson.name }}
{{ lesson.shortDescription }}
(click)="downloadJson()"
(click)="saveLessonPlan()"
[disabled]="isSaving"
*ngIf="isSaving"
*ngIf="saveSuccess"
*ngFor="let type of lessonTypes"
```

**Redesign notes:**
- Form should feel modern and clean
- Generated lessons should be visually distinct cards
- Consider stepper or wizard feel (fill form → see results → save)
- Loading state should feel polished

---

### 4. Lesson Calendar — `lesson-days.html` + `lesson-days.css`

**Current structure:**
- Header with title + subtitle
- Two-column grid:
  - Left: Assignment form card (select plan → select lesson → pick date → optional name/description → assign button)
  - Right: Calendar card with month navigation + list of lesson days (each day shows assigned lessons with unassign button)

**Angular bindings to preserve:**
```
[(ngModel)]="selectedLessonPlan"
(ngModelChange)="onLessonPlanChange()"
*ngFor="let plan of lessonPlans"
*ngIf="selectedLessonPlan"
[(ngModel)]="selectedLesson"
*ngFor="let lesson of availableLessons"
[disabled]="lesson.isAssigned"
{{ lesson.isAssigned ? ' (Already assigned)' : '' }}
[(ngModel)]="selectedDate"
(dateChange)="onDateChange($event.value)"
[matDatepicker]="picker"
[(ngModel)]="dayName"
[(ngModel)]="dayDescription"
*ngIf="error"
*ngIf="successMessage"
(click)="resetForm()"
(click)="assignLesson()"
[disabled]="!selectedLesson || isAssigning"
*ngIf="isAssigning"
(click)="previousMonth()"
{{ currentMonthName }}
(click)="nextMonth()"
*ngFor="let day of lessonDays"
{{ day.name }}
{{ day.date | date:'fullDate' }}
{{ day.shortDescription }}
*ngFor="let lesson of day.lessons"
{{ lesson.lessonNumber }}
{{ lesson.name }}
{{ lesson.lessonPlanName }}
(click)="unassignLesson(lesson.id)"
*ngIf="lessonDays.length === 0"
```

**Redesign notes:**
- Consider a real calendar grid view instead of a list
- Month navigation should feel intuitive
- Assigned lessons should be visually grouped by day
- Drag-and-drop would be a bonus (but optional — would need component logic changes)

---

### 5. Lesson Detail — `lesson-detail.html` + `lesson-detail.css`

**Current structure:**
- Loading spinner
- Error state (icon + message + back button)
- Main content:
  - Back button
  - Card with lesson number avatar, name, description
  - Lesson content rendered as markdown
  - Exercises section:
    - Difficulty select + Generate Exercise button (with spinner)
    - Expansion panels per exercise:
      - Exercise text (markdown)
      - Answer input (textarea + submit button with spinner)
      - Previous submissions list (user answer, accuracy %, review markdown)

**Angular bindings to preserve:**
```
*ngIf="isLoading"
*ngIf="error"
{{ error }}
routerLink="/today"
*ngIf="lesson"
{{ lesson.lessonNumber }}
{{ lesson.name }}
{{ lesson.shortDescription }}
*ngIf="lesson.content"
[data]="lesson.content"
[(ngModel)]="selectedDifficulty"
*ngFor="let d of difficulties"
(click)="generateExercise()"
[disabled]="isGeneratingExercise"
*ngIf="isGeneratingExercise"
*ngFor="let exercise of lesson.exercises; let i = index"
{{ i + 1 }}
{{ exercise.difficulty }}
[data]="exercise.exerciseText"
[(ngModel)]="answerTexts[exercise.id]"
[disabled]="submittingExerciseId === exercise.id"
(click)="submitAnswer(exercise)"
[disabled]="submittingExerciseId === exercise.id || !answerTexts[exercise.id]?.trim()"
*ngIf="submittingExerciseId !== exercise.id"
*ngIf="submittingExerciseId === exercise.id"
*ngIf="exercise.answers && exercise.answers.length > 0"
*ngFor="let answer of exercise.answers"
{{ answer.userResponse }}
*ngIf="answer.accuracyLevel !== null && answer.accuracyLevel !== undefined"
{{ answer.accuracyLevel }}
*ngIf="answer.reviewText"
[data]="answer.reviewText"
```

**Redesign notes:**
- This is the most content-heavy page — readability is key
- Lesson content (markdown) needs good typography
- Exercises should feel like interactive cards, not just expandable panels
- Answer submissions should show accuracy visually (progress bar, color coding)
- Consider a sidebar TOC for long lessons

---

## Current Color Palette

| Usage | Color |
|-------|-------|
| Primary gradient | `#667eea → #764ba2` (purple) |
| Success | `#2e7d32` / `#e8f5e9` |
| Error | `#c62828` / `#ffebee` |
| Info/Accent | `#1976d2` / `#e3f2fd` |
| Lesson avatar | `#673ab7` |
| Text dark | `#333` |
| Text medium | `#666` |
| Text light | `#999` |
| Background | `#f5f5f5` / `#fafafa` |
| Border | `#ccc` / `#ddd` |

---

## Angular Material Components Used

You may restyle but must keep these component selectors:

- `mat-card`, `mat-card-header`, `mat-card-content`, `mat-card-actions`
- `mat-button`, `mat-raised-button`, `mat-icon-button`
- `mat-icon`
- `mat-form-field` (with `appearance="outline"`)
- `mat-input` / `matInput`
- `mat-select` / `mat-option`
- `mat-datepicker` / `matDatepicker`
- `mat-expansion-panel` / `mat-accordion`
- `mat-spinner`
- `mat-progress-bar`
- `mat-list` / `mat-list-item`
- `mat-chip` / `mat-chip-set`
- `mat-divider`
- `<markdown>` (ngx-markdown)

---

## Constraints

1. **Do NOT modify** `.ts` files, services, models, or routing
2. **Do NOT remove or rename** any Angular binding (`*ngIf`, `*ngFor`, `[(ngModel)]`, `(click)`, `[routerLink]`, etc.)
3. **Do NOT remove** any Angular Material component — you can restyle them with CSS but the selectors must stay
4. **You CAN:**
   - Restructure HTML (add wrapper divs, reorder sections, add CSS classes)
   - Completely rewrite CSS files
   - Change the layout, spacing, colors, typography, animations
   - Add new CSS classes to HTML elements
   - Change `appearance` attribute on mat-form-fields
   - Add new Angular Material components if needed (they're already imported)
5. **Responsive design** is required (mobile-first preferred)
6. **Dark mode** support is optional but welcome

---

## Files to Deliver

```
app.html          + app.css
todays-lessons.html + todays-lessons.css
lesson-plan.html    + lesson-plan.css
lesson-days.html    + lesson-days.css
lesson-detail.html  + lesson-detail.css
```

Total: **5 HTML files + 5 CSS files**

---

## Design Direction (Suggestions — feel free to propose alternatives)

- Clean, modern educational app feel (think Notion, Duolingo, or Coursera)
- Good whitespace and content hierarchy
- Smooth micro-interactions (hover, focus, transitions)
- Clear visual feedback for loading, success, and error states
- Accuracy scores should use color coding (red < 50%, yellow 50-80%, green > 80%)
- Cards should have subtle depth (shadow or border, not both)
- Typography: clear hierarchy with distinct heading sizes
- Consider using CSS custom properties (variables) for easy theming
