function a() {

	$('#search-btn').click(function () {
		$('.text-success').text('');
		$('#searchForm').submit(function (e) {
			// ページの更新を禁止する
			e.preventDefault();
			// 送信イベントが他のハンドラーに伝播するのを防ぐ
			e.stopImmediatePropagation();

			// フォーム情報取得
			let url = window.location.origin + '@Url.Action("SearchData", "D_ScanRecord")';
			let method = 'POST';
			let data = $(this).serialize();

			$.ajax({
				url: url,
				method: method,
				data: data
			}).done(function (response) {
				// 検索結果表示
				$("#table-datatable tbody").empty();
				$("#div-table").show();
				$("#table-datatable tbody").html(response);
			}).fail(function (xhr, textStatus, errorThrown) {
				if (xhr.status === 404) {
					$("#div-error-message").text('データ更新に失敗しました');
				} else {
					$("#div-error-message").text('サーバーに接続できませんでした');
				}
			});
		});
	});
	//読取・送信日付の値がnullの場合、今日の日付を設定する
	$(function () {
		$('#AGFMotionTextColorStart').datepicker({
			format: 'yyyy-mm-dd',
			language: 'ja',
			autoclose: true,
			todayHighlight: true,
		});
		$('#AGFMotionTextColorEnd').datepicker({
			format: 'yyyy-mm-dd',
			language: 'ja',
			autoclose: true,
			todayHighlight: true,
		});
		if ($('#AGFMotionTextColorStart').val() == '') {
			$('#AGFMotionTextColorStart').datepicker('setDate', new Date());
		}
		if ($('#AGFMotionTextColorEnd').val() == '') {
			$('#AGFMotionTextColorEnd').datepicker('setDate', new Date());
		}
	});
}