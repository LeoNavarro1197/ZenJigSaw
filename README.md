# Rompecabezas - Zen Jigsaw

Juego móvil de **rompecabezas 2D con enfoque "zen" / relajante**. El jugador ensambla
fragmentos de una foto en un tablero, con una bandeja de piezas barajadas, sistema de
pistas, zoom y paneo. Además permite convertir cualquier foto de la galería del
dispositivo en un rompecabezas personalizado.

- **Product name:** `Rompecabezas - Zen Jigsaw`
- **Package ID:** `com.gasstation.zenjigsaw`
- **Versión:** 1.0 (Android bundle version code `2`)

---

## Stack técnico

| Área | Tecnología |
|---|---|
| Motor | Unity **6000.4.0f1** |
| Render pipeline | URP 17.4.0 (template 2D) |
| UI | **uGUI** + TextMeshPro (no se usa UI Toolkit) |
| Input | Input System 1.19.0 (`EnhancedTouch` para pinch-zoom) |
| Animación | DOTween (Demigiant) |
| Backend | Firebase 13.15.0 — App, Auth, Firestore, Storage |
| Anuncios | Google Mobile Ads 11.3.0 (banner, interstitial, rewarded) |
| Compras | Unity IAP 5.4.2 (producto único `remove_ads`) |
| Galería nativa | NativeGallery 1.9.4 (yasirkula) |
| Gestión de deps Android | Google External Dependency Manager 1.2.188 |

**Plataforma objetivo:** Android (Google Play), portrait, IL2CPP, **ARM64-v8A únicamente**,
minSdk 25 (Android 7.1), immersive fullscreen. iOS está sin configurar.

---

## Estructura del proyecto

```
Assets/
  Scenes/         MainMenu.unity (4 pestañas) · GameScene.unity (tablero)
  Scripts/
    Core/         PuzzleManager, AdsManager, IAPManager, FirebaseManager,
                  AudioManager, HapticManager, PuzzleDataCarrier
    Puzzle/       ImageSlicer (genera piezas) · PuzzlePiece (drag & drop)
                  BoardZoomPan (zoom/pan)
    Data/         PuzzleLevelData (ScriptableObjects) · SaveSystem (JSON)
    UI/           Home, Collection, Custom, Crop, MenuNavigator, Settings...
  Prefabs/        Piece_Prefab · PuzzleCard_Prefab · PuzzleCardCollection_Prefab
  ScriptableObjects/  NaturePack · AnimalsPack · LandscapePack
  Shader/         JigsawBevel.shader (bisel 3D de las piezas)
  Art/            Fotos de los packs, fondos del tablero, UIGifPlayer frames
```

### Sistemas principales

- **Generación de piezas** — `Puzzle/ImageSlicer.cs` calcula la tabla de tabs/blanks
  (vecinos siempre complementarios, bordes planos) y corta cada pieza píxel a píxel en
  CPU. La rejilla se deriva como `cols = ceil(sqrt(n))`, `rows = ceil(n / cols)`.
- **Drag & drop** — `Puzzle/PuzzlePiece.cs` arbitra entre scroll de la bandeja y arrastre
  de la pieza, reparenta de `trayContent` a `boardArea` conservando posición mundial y
  hace *snap* con DOTween + háptica.
- **Guardado** — `Data/SaveSystem.cs` escribe un JSON por puzzle en
  `Application.persistentDataPath/<puzzleId>.json` con piezas colocadas, completitud,
  cols/rows, pistas restantes y timestamp.
- **Contenido remoto** — `Core/FirebaseManager.cs` consulta la colección Firestore
  `puzzles` y descarga las imágenes vía `UnityWebRequest`, cacheándolas como JPG local.
- **Puzzles personalizados** — `UI/CustomPuzzleManager.cs` + `UI/CropManager.cs`:
  galería → recorte cuadrado → JPG en `persistentDataPath`.

---

## Guía de instalación (clone nuevo)

```bash
git clone https://github.com/LeoNavarro1197/ZenJigSaw.git
```

1. Abrir con **Unity 6000.4.0f1** (Hub muestra la versión exacta).
2. Esperar a que termine el import. La primera compilación tarda (hay ~250 MB de SDKs
   de Firebase y AdMob commitados).
3. En el Editor, ir a `Assets > External Dependency Manager > Android Resolver > Resolve`.
   Regenera los AARs de Android y el `mainTemplate.gradle`.
4. Compilar con el Build Profile `Android™` (il2CPP, Release).

> **Nota:** el paso 3 es obligatorio. Firebase y Google Mobile Ads distribuyen sus
> dependencias Android mediante un repositorio Maven local que se reconstruye en cada
> máquina.

---

# `.gitignore`: qué se excluye y por qué

El archivo sigue la [plantilla oficial de Unity](https://github.com/github/gitignore/blob/main/Unity.gitignore),
con dos bloques propios al final. A continuación el detalle completo.

## 1. Generado por Unity (plantilla estándar)

Estas carpetas las recrea el Editor automáticamente. Commitearlas es la causa #1 de
repositorios Unity gigantes y de conflictos de merge constantes.

| Patrón | Tamaño local | Motivo |
|---|---|---|
| `/[Ll]ibrary/` | **12.4 GB** | Cache de importación, shaders y build cache. **Es el mayor responsable del tamaño.** |
| `/[Ll]ogs/` | 3.3 MB | Logs del Editor. |
| `/[Tt]emp/`, `/[Oo]bj/` | — | Archivos temporales de importación. |
| `/[Uu]ser[Ss]ettings/` | 0.1 MB | Preferencias locales del Editor. |
| `/[Bb]uild/`, `/[Bb]uilds/` | 0 MB | Salida de build. |
| `/.utmp/` | 2.3 MB | Temp del Unity Editor Services. |
| `.vs/` | 2.2 MB | Estado del Visual Studio / Rider. |
| `*.csproj`, `*.sln`, `*.slnx`, `*.unityproj` | ~1 MB | Generados por el IDE, no por Unity. |
| `*.apk`, `*.aab`, `*.unitypackage`, `*.app` | — | Artefactos de build y paquetes exportados. |
| `*.pdb`, `*.mdb` | ~30 MB | Símbolos de depuración de DLLs (Firebase, EDM, DOTween). |
| `*.log`, `*.tmp`, `*.user`, `*.suo` | — | Ruido de IDE y crash dumps. |
| `sysinfo.txt`, `mono_crash.*` | — | Dumps de crash. |
| `/[Mm]emoryCaptures/`, `/[Rr]ecordings/` | — | Capturas de pantalla; pueden ser enormes. |
| `/[Aa]ssets/Addressables*` | — | Project no usa Addressables, pero queda por si se adopta. |
| `/[Aa]ssets/Unity.VisualScripting.Generated/**` | — | Project no usa Visual Scripting. |
| `/[Aa]ssets/[Ii]nit[Tt]est[Ss]cene*.unity*` | — | Escenas de Test Runner. |

## 2. Exclusiones específicas de este proyecto

```
# Firebase: native library for macOS Desktop (Standalone)
/Assets/Firebase/Plugins/x86_64/FirebaseCppApp-13_15_0.bundle
/Assets/Firebase/Plugins/x86_64/FirebaseCppApp-13_15_0.bundle.meta
```

| Archivo | Tamaño | Motivo |
|---|---|---|
| `FirebaseCppApp-13_15_0.bundle` | **106.4 MB** | Binario nativo de **macOS Desktop**. El proyecto solo compila para Android, así que nunca se empaqueta. |
| `FirebaseCppApp-13_15_0.bundle.meta` | — | Se ignora junto con su asset para no dejar un `.meta` huérfano (Unity lo regenera). |

Se ignoran el `.bundle` **y** su `.meta` deliberadamente: si solo se ignorara el binario,
el `.meta` quedaría versionado sin su asset, y Unity perdería el GUID de referencia al
reimportar.

### Contexto: el resto de Firebase sí se versiona a propósito

`Assets/Firebase/` pesa ~250 MB en disco, pero solo se ignoran los 106 MB del `.bundle`.
Esto es deliberado: **un clone nuevo debe compilar sin pasos manuales de re-importación.**

| Carpeta | Tamaño | ¿Se versiona? | Motivo |
|---|---|---|---|
| `Assets/Firebase/Plugins/x86_64/` (resto) | ~112 MB | Sí | `.dll` de Windows, `.so` de Android x86_64, `.dll`/`.bundle` de Auth/Firestore/Storage. El build es ARM64-only, pero se versionan para no romper referencias de plataforma. |
| `Assets/Firebase/m2repository/` | 24.5 MB | Sí | AARs de Android (`firebase-app-unity-13.15.0.srcaar`, 21.8 MB). **Requeridos para compilar.** |
| `Assets/Firebase/Editor/` | 6.1 MB | Sí | Integración EDM/gradle. **Requerida para el build de Android.** |
| `Assets/Plugins/Android/` | 0.2 MB | Sí | `mainTemplate.gradle` y templates de gradle. **Requeridos.** |
| `Assets/GeneratedLocalRepo/` | 24.5 MB | Sí | Repo Maven local de Google Play Services, resuelto por EDM. |
| `Assets/GoogleMobileAds/` | 1.6 MB | Sí | Plugin de AdMob, incluido su `.aar`. |
| `Assets/Plugins/iOS/`, `tvOS/` | 35.4 MB | Sí | Actualmente no hay build para esas plataformas, pero se versionan por completitud del SDK. |

---

## Oportunidades de ahorro pendientes

No se aplicaron todavía (requieren decisión, ya que rompen la compilación de un clone
nuevo o borran archivos del disco). Están medidas y verificadas:

| Carpeta | Tamaño | Acción sugerida |
|---|---|---|
| `Assets/Firebase/Plugins/x86_64/` completo | **218.8 MB** | Es 100% muerto: `.bundle`/`.dll` de Desktop y `.so` de Android x86_64 (excluido del APK por `mainTemplate.gradle` y por `AndroidTargetArchitectures: 2`). Las nativas Android de Firebase vienen de `m2repository/`. |
| `Assets/Plugins/tvOS/` | 15.2 MB | No hay build tvOS. |
| `Assets/Plugins/iOS/*.a` | 15.3 MB | 4 librerías estáticas; solo se necesitan para iOS. |
| `Assets/Firebase/m2repository/` + `Assets/GeneratedLocalRepo/` | 49 MB | Se regeneran con `Android Resolver > Resolve` (ver guía de instalación). |
| **Total recuperable** | **~299 MB** | |

Además, conviene mover a secrets/`.gitignore` antes del próximo commit:

- `user.keystore` — keystore de firma Android, alias `leonardo`.
- `Assets/google-services.json` — contiene la API key de Firebase (normal en clientes,
  pero conviene saber que queda expuesta en un repo público).

---

## Notas de estado

- AdMob usa los **IDs de prueba** de Google (`ca-app-pub-3940256099942544/...`) tanto en
  `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` como en
  `Core/AdsManager.cs`. Hay que reemplazarlos antes de publicar.
- `ScriptableObjects/LandscapePack.asset` está **vacío**, pero la pestaña "Paisaje"
  existe en el menú.
- Código muerto: `PuzzleManager.puzzleMasks` (campo serializado vacío y sin uso),
  `Assets/Art/JigsawMaskTemplate.png` y `Assets/Art/mascara.png`.
- No hay pipeline de build automatizado: todo se compila a mano desde la ventana de
  Build Profiles, y no existe carpeta `Assets/Editor`.
- `ImageSlicer.Slice` hace un `GetPixel` por téxel por pieza en C# (hasta 225 piezas),
  de forma síncrona en `Start()`. Es el candidato #1 a optimizar si hay tirones al cargar.
