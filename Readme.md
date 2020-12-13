# Task-Based State Machine (TBSM) Library
8/14/2020 10:49:15 AM 

Task-Based State Machine 是用來解決一般使用 While + Switch 來做 State Machine 擴充性及遷移性過差的情形。 以往的方式會發現當 State Machine 流程更改時，或想複製部分功能至其他地方時，會需要大量的複製貼上，導致多處程式碼的重複及維護上的困難(需記得同時更該多個地方)。因此，此 Task-Based State Machine 包含以下特點來解決上述問題:

1.	**所有 狀態(state) 都將打包成 method。 這些 method 可依照流程連接。** 因此解決了擴充性及遷移性的問題。
2.	**流程將以 C# Task 多執行續的方式去跑。** 並在此 Library 中做好相對應 Thread-Safe 的保護機制。
3.	**提供錯誤處理機制。** 此錯誤處理機制讓此 State Machine 可以在預期或非預期例外發生時，安全結束 State Machine，並且可以做相對應的處理。
4.	**不同 TBSM 之間串聯。** 當流程過於龐大時，可以拆成許多小的流程再彼此串聯。 便於將流程模組化。


# 程式路徑

目前於 SVN 上的位置如下:

**http://yoosap.youngoptics.com/svn/nvs/DotNet Plugin/TaskBasedStateMachine**

此 Library 程式的進入點位於 TaskBasedStateMachine/TaskBasedStateMachineTest/TaskBasedStateMachineTest.sln。為此程式庫的範例。 
TaskBasedStateMachineLibrary 在編譯後的輸出路徑為 **./OutputDlls**。


# 使用說明
## DLL 參考
使用時只需參考 **TaskBasedStateMachineLibrary.dll** 即可。

但是同樣的dll 路徑底下，
需含有底下五個相依的 dll:

 -	**Newtonsoft.Json.dll**
 -	**Newtonsoft.Json.Schema.dll**
 -	**DotNetZip.dll**
 -	**Ninject.dll**
 -	**LoggerManagerLibrary.dll**
 
此五者皆可在 TaskBasedStateMachine/TaskBasedStateMachineLibrary/OutputDlls 路徑底下找到。

另外，由於LibreOffice 輸出需要一 **template.ods** 檔案，需要一併複製到輸出路徑底下，使用時會需要指定template路徑。

**註：在 OutputDlls 中另有一個 external 資料夾。 此資料夾是為繪製及輸出 TBSM 的流程圖所用。** 可以將此資料夾複製並匯入您的專案底下，並將其中的所有內容屬性設置為 **一律複製**，或者是直接將此資料夾放置到輸出路徑中，即可輸出流程圖。


## 基本使用範例

TBSM 的使用分成三步驟:

1.	**準備傳入參數。** TBSM 提供一個方法將參數在不同 State 中傳遞。 
2.	**準備所有需要使用到的方法(methods)。** 這些方法即為 TBSM 中的 **"狀態 (States)"**。
3.	**定義流程。** 建構出 TBSM 的流程。 可以使用 TBSM.Draw() 方法劃出流程，以便確定流程是否正確。

底下將會詳細帶您走過 TBSM 的建構及使用流程。

### 1. 準備傳入參數 

在跑 TBSM 的過程中，難免會需要有參數在之中傳遞，因此在進到第二步 **"狀態定義"**
之前，需要先準備參數。 

參數的準備，**僅需讓一個類別去繼承 IParameterClass 介面即可**。
此類別中僅含有 

1. **Public 屬性** (所有需要用到的參數集中管理)
2. **Public 常數**

>

	// 準備一個自訂類別，讓它繼承 Library 中的 IParameterClass，
	// 並且實作此 IParameterClass 中的所有屬性
	public class MyParam : IParameterClass
    {
		// 常數
		public const int Timeout = 10;

		// 此為 IParameterClass 中的屬性
        public string NextState { get; set; }

		// 此為 IParameterClass 中的屬性
        public CancellationToken cancellationToken { get; set; }

		// 此為自訂義屬性
        public string StringFromPreivousTask;

    }

> **1. IParameterClass 中有兩個需要實作的屬性: NextState 及 CancellationToken**
>> 1.1. **NextState:** 下一個要進入的狀態。 若不指定，則會使用預設的狀態。 (詳見第三步 **定義流程**)。  
>> 1.2. **CancellationToken:** 中斷 Task 的物件。 由於 TBSM 是於 C# Task 的基礎上建構，而中斷一個 Task 最好的方法就是使用這個 CancellationToken。 欲指定這個物件，需在外部**先定義一個 CancellationTokenSource**，然後再將此 CancellationTokenSource.Token 傳入至 CancellationToken 即可。

>>
	// Global Variable
	CancellationTokenSource cts = new CancellationTokenSource();
	CancellationToken token = cts.Token;
>>
	// 將 Token 傳入 Task
    Task t = new Task(() => { // Some Ansync Work}, token);
>>
	// 於程式中任一處呼叫取消工作
	cts.Cancel(); // => t 會被終止。

>> **註1: cts 只能使用一次。一旦呼叫 Cancel 後就必須再重新 new 出一個 CancellationTokenSource()**;

>> **註2: TBSM 中會自動把 CancellationToken 傳入所有使用到的 Task**，因此使用時僅需傳入 Token 即可，不需要自訂 Task。

> **2. 常數**。 若再 TBSM 中有需要用到某常數 (比如說 Timeout 次數，預設數值...)，可以在自訂類別中使用 const 來定義。 const 修飾詞的使用方式如下:
 
>>
	// 使用常數的方式與一般屬性不同
	public void SomeClass(MyParam param)
	{
		// 使用一般屬性
		param.NextState = "SomeState";
>>
		// 使用常數的方式如同使用 Static 屬性
		int t = MyParam.Timeout;
	}

### 2. 狀態(States)定義

由於需要有參數的傳遞，因此需要限定如何定義狀態。所有的狀態皆為一個方法(method)，**傳入 parameter object 並回傳 parameter object**。(此 parameter object 即為第一步定義的參數。)


	// 傳入 MyParam 並 回傳 MyParam (MyParam 定義於第一步)
    public MyParam Dummy(MyParam p)
    {
        // Do something for this function

		// Return MyParam
        return p;
    }

狀態的內容有幾件事可以做:

1.	**決定下一步**
2.	**例外處理**

> **1. 決定下一步**:
> 在第三步驟的 **"定義流程"** 中，會定義出 TBSM 的流程。 而在其中的某些狀態會需要根據某些狀況決定下一個狀態，而決定下一個流程的方法即是利用 **IParamterClass 中的 NextState**。

>>  
	public MyParam DummyWithConditions(MyParam p)
    {
		// 假設有個狀況會回傳一個reult 結果為 1~3，分別需要對應到三個不同的狀態
		int result = SomeOtherWorks();
>>
		// 根據回傳結果決定下一步
		// 注意: 第一個連接在此狀態後的狀態 (此處假設為 Step 1) 為預設，不需要特別指定
		switch(result)
		{
			case 2:
				// 指定下一狀態的名稱 (此StepTwo 需要在第三步中確實連接在此狀態的後面)
				p.NextState = nameof(StepTwo);
				break;
>>
			case 3:
				// 亦可利用 index 來決定下一狀態
				// 方法是呼叫 tbsm.Flow, 取出現在的狀態之後所連接的狀態(List)，並指定index (第三步 index 為 2)
				p.NextState = tbsm.Flow[tbsm.CurrentTaskName][2];
				break;
		}
>>
		// 回傳參數
        return p;
    }

> **2. 例外處理:** 
>  例外處理分兩部分: **HandledException** 及 **UnhandledException**。

> **1. HandledException:**  預期中的例外狀況一般可藉由 Try/Catch 或自行條件判斷觸發，而定義在 HandledException 中的所有 Methods 都可以在任意狀態中呼叫 (詳見 第三步驟的 **"定義流程"**)
>> **註: 預期中的例外處理方法與其他一般方法的定義一樣**(同樣是輸入 IParamterClass Object 回傳 IParameterClass Object)，**但由於可被任意狀態呼叫，因此也可以用來做收尾步驟。** 而 **所有HandledException Methods 都是終點，不會再有下一步。**

> **2. UnhandledException:** 非預期中的例外可以是在某個**狀態中發生的非預期狀況**，或者是 **TBSM Library 中發生的非預期狀況**。 無論是何種，一旦有非預期例外狀況發生，都會進入到 UnhandledExceptionState (可於第三步 **"定義流程"** 中定義)。 此方法的寫法與其他一般方法一致(同樣是輸入 IParamterClass Object 回傳 IParameterClass Object)，但 **不可被呼叫(也就是無法藉由 NextState 指定到此狀態)**。


### 3. 定義流程

整套TBSM 流程定義分三步驟:

1.	**創建 TBSM 物件**
2.	**給定所有需要用到的狀態方法。**
3.	**指定流程。**


>**1. 創建 TBSM 物件**

> 創建 TBSM 物件時，**需要同時給定傳入參數的類別**(如第一步所示)
>> 
	// TBSM<TParam> 中的 TParam 即是第一步中所建立的類別
    TaskBasedStateMachine<MyParam> tbsm = new TaskBasedStateMachine<MyParam>();

>**2. 給定所有方法**

>創建好 TBSM 物件之後，**需要給定會使用到的所有狀態方法** (如第二步所定義)。 給定的方法即是使用 TBSM 中的 **SetupTasks** 方法。
>>
	// 使用 SetupTasks 定義所有之後會使用到的發法(即，狀態)
 	tbsm.SetupTasks(new List<(string, Func<MyParam, MyParam>)> {
        (nameof(A), A.Dummy),
        (nameof(B), B.Dummy),
        (nameof(C), C.DummyWithConditions),
        (nameof(D), D.Dummy),
        (nameof(E), E.DummyWithConditions),
        (nameof(F), F.DummyWithUnhandledException),
        (nameof(G), G.Dummy),
        (nameof(H), H.Dummy),
        (nameof(I), I.Dummy),
        (nameof(J), J.Dummy),
        (nameof(K), K.DummyWithHandledException),
        (nameof(L), K.DummyWithAnotherHandledException),
        (nameof(HandledExceptionTask), HandledExceptionTask),
        (nameof(AnotherHandledExceptionTask), AnotherHandledExceptionTask)
        });
>> **SetupTasks:** 傳入一 **List<string, Func<MyParam, MyParam>** 物件。 
>>
>> - 此 List 中第一個參數為 **State 名稱**。 String Type。
>> - 此 List 中第二個參數為 **Func<MyParam, MyParam>**。 即 第二步所定義的方法本身。
>> 
>> **註:** 由於一般而言，**狀態方法的名稱常直接就是狀態名稱**，因此建議在給定狀態名稱時，使用 **nameof()** 方法，減少出錯且便於更改名稱。

>**3. 定義流程**

>以下介紹所有 TBSM 中定義的建置流程的方法:

> 1. **StartWith:** 第一步。 **傳入第一個狀態的狀態名稱**。所有TBSM 物件建立流程的第一步都一定是起始於StartWith 方法，**且只會使用一次**。
> 2. **FollowedBy:** 下一個狀態。 **傳入下一個狀態的狀態名稱**。
> 3. **FollowedByASeriesOfTasks:** 接入一串流程。 **傳入另一個已經定義好流程的 TBSM 物件。** 直接在此狀態下接入一串其他定義好的流程，方便模組化。
> 4. **ConditionalFlow:**
> 5. **AddHandledExceptionTasks:**
> 6. **AddUnhandledExceptionTask:**